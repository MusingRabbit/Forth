using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Level;
using Assets.Scripts.Match;
using Assets.Scripts.Network;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MatchManager : MonoBehaviour
    {
        private bool m_matchReady;
        private bool m_initalised;

        private static MatchManager _instance;
        private IGameManager m_gameManager;

        public event EventHandler<EventArgs> OnInitialisation;
        public event EventHandler<EventArgs> OnMatchStateChanged;
        public event EventHandler<EventArgs> OnMatchTypeChanged;

        public event EventHandler<OnPlayerAddedArgs> OnPlayerAdded;
        public event EventHandler<OnPlayerSwitchTeamsArgs> OnPlayerTeamSwitch;
        public event EventHandler<OnPlayerRemovedArgs> OnPlayerRemoved;

        private MatchData m_matchData;
        private Dictionary<ulong, PlayerMatchData> m_players;
        private Dictionary<ulong, PlayerMatchData> TeamBluePlayers => m_matchData.Teams[Team.Blue].Players;
        private Dictionary<ulong, PlayerMatchData> TeamRedPlayers => m_matchData.Teams[Team.Red].Players;

        private List<FlagBase> m_flagBases;

        public bool IsReady
        {
            get
            {
                return m_matchReady;
            }
        }

        public IGameManager GameManager
        {
            get
            {
                return m_gameManager;
            }
        }

        public static MatchManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public MatchManager()
        {
            m_matchData = new MatchData();
            
            m_players = new Dictionary<ulong, PlayerMatchData>();

            m_matchData.MatchState = MatchState.Ended;
            this.OnMatchStateChanged += this.MatchManager_OnMatchStateChanged;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                GameObject.DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                GameObject.Destroy(base.gameObject);
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_matchData.MatchType = MatchType.Deathmatch;
            m_matchData.MatchState = MatchState.Ended;
            m_matchData.ScoreLimit = 5;

            this.SubscribeToNotifications();
        }

        // Update is called once per frame
        private void Update()
        {
            switch (m_matchData.MatchState)
            {
                case MatchState.PendingStart:
                    this.PendingStartUpdate();
                    break;
                case MatchState.Running:
                    this.RunningUpdate();
                    break;
                case MatchState.Ended:

                    break;
            }
        }

        private void SubscribeToNotifications()
        {
            NotificationService.Instance.OnPlayerKilled += this.Notification_OnPlayerKilled;
        }

        private void PendingStartUpdate()
        {
            switch (m_matchData.MatchType)
            {
                case MatchType.Deathmatch:
                    if (m_matchData.Teams[Team.None].Players.Count > 1)
                    {
                        m_matchData.MatchState = MatchState.Running;
                        this.OnMatchStateChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case MatchType.TeamDeathmatch:
                case MatchType.CaptureTheFlag:
                    if (this.TeamBluePlayers.Any() && this.TeamRedPlayers.Any())
                    {
                        m_matchData.MatchState = MatchState.Running;
                        this.OnMatchStateChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        private void RunningUpdate()
        {
            m_matchData.CurrentTime += Time.deltaTime;
            var timeLimitReached = m_matchData.TimeLimit > TimeSpan.Zero && m_matchData.CurrentTime >= m_matchData.TimeLimit.TotalSeconds;
            bool scoreLimitReached = false;

            switch (m_matchData.MatchType)
            {
                case MatchType.Deathmatch:
                    var topPlayer = this.GetTopPlayerMatchData(Team.None);

                    if (topPlayer == null)
                    {
                        NotificationService.Instance.Warning("GetTopPlayerMatchData() returned null.");
                        return;
                    }

                    scoreLimitReached = topPlayer.Score >= m_matchData.ScoreLimit;
                    break;
                case MatchType.TeamDeathmatch:
                case MatchType.CaptureTheFlag:
                    var redTeamScore = m_matchData.Teams[Team.Red].TeamScore;
                    var blueTeamScore = m_matchData.Teams[Team.Blue].TeamScore;
                    var maxScore = Math.Max(redTeamScore, blueTeamScore);
                    scoreLimitReached = maxScore >= m_matchData.ScoreLimit;
                    break;
            }

            if (timeLimitReached || scoreLimitReached)
            {
                m_matchData.MatchState = MatchState.Ended;
                this.OnMatchStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Notification_OnPlayerKilled(object sender, OnNotificationEventArgs e)
        {
            var killedData = e.Data.GetData<PlayerKilledData>();

            var killerState = killedData.Killer.GetComponent<ActorState>();
            var killedState = killedData.Killed.GetComponent<ActorState>();

            switch (m_matchData.MatchType)
            {
                case MatchType.TeamDeathmatch:
                    var score = killedState.Team != killerState.Team ? 1 : -1;

                    this.AddTeamScore(killerState.Team, score);
                    break;
            }


            this.AddPlayerScore(killedData.Killer, 1);
        }

        public void Initialise(IGameManager gameManager)
        {
            m_gameManager = gameManager;
            this.OnInitialisation?.Invoke(this, EventArgs.Empty);
        }

        public void InitialiseMatch(MatchSettings settings, MatchState matchState = MatchState.PendingStart)
        {
            m_matchData = new MatchData();
            m_matchData.MatchType = settings.MatchType;
            m_matchData.ScoreLimit = settings.ScoreLimit;
            m_matchData.TimeLimit = settings.TimeLimit;

            m_matchData.MatchState = matchState;

            NotificationService.Instance.Info($"MatchType:{m_matchData.MatchType}|MatchState:{m_matchData.MatchState}|ScoreLimit:{m_matchData.ScoreLimit}|TimeLimit:{m_matchData.TimeLimit}");

            this.InitialiseMatch(m_matchData);
        }

        public void InitialiseMatch(MatchData data)
        {
            m_matchData = new MatchData();
            m_matchData.Teams = new Dictionary<Team, TeamData>();

            m_matchData.MatchType = data.MatchType;
            m_matchData.ScoreLimit = data.ScoreLimit;
            m_matchData.TimeLimit = data.TimeLimit;

            m_matchData.MatchState = data.MatchState;

            Dictionary<ulong, PlayerMatchData> players = new Dictionary<ulong, PlayerMatchData>();

            switch (m_matchData.MatchType)
            {
                case MatchType.Deathmatch:
                    m_matchData.Teams.Add(Team.None, new TeamData(Team.None));

                    if (data.Teams.ContainsKey(Team.None))
                    {
                        m_matchData.Teams[Team.None] = data.Teams[Team.None];
                        players.AddRange(data.Teams[Team.None].Players);
                    }

                    break;
                case MatchType.TeamDeathmatch:
                case MatchType.CaptureTheFlag:
                    m_matchData.Teams.Add(Team.Red, new TeamData(Team.Red));
                    m_matchData.Teams.Add(Team.Blue, new TeamData(Team.Blue));

                    if (data.Teams.ContainsKey(Team.Blue))
                    {
                        players.AddRange(TeamBluePlayers);
                    }

                    if (data.Teams.ContainsKey(Team.Red))
                    {
                        players.AddRange(TeamRedPlayers);
                    }

                    if (m_matchData.MatchType == MatchType.CaptureTheFlag)
                    {
                        m_flagBases = this.GetAllFlagBasesInScene(SceneManager.GetActiveScene());

                        foreach (var fbase in m_flagBases)
                        {
                            fbase.FlagCaptured += FlagBase_FlagCaptured;
                        }
                    }

                    break;
            }


            m_players = players;
            m_matchReady = true;
        }

        private void FlagBase_FlagCaptured(object sender, EventArgs e)
        {
            var fbase = (FlagBase)sender;
            var team = fbase.Team == Team.Blue ? Team.Red : fbase.Team == Team.Red ? Team.Blue : Team.None;
            this.AddTeamScore(team, 1);
        }

        private List<FlagBase> GetAllFlagBasesInScene(Scene scene)
        {
            var result = new List<FlagBase>();
            var rootObjs = scene.GetRootGameObjects();

            foreach(var obj in rootObjs)
            {
                var flagBases = obj.GetComponentsInChildren<FlagBase>();
                result.AddRange(flagBases);
            }

            return result;
        }



        public IReadonlyMatchData GetMatchData()
        {
            return m_matchData.ToReadonlyMatchData();
        }

        public bool PlayerExists(ulong clientId)
        {
            return m_players.ContainsKey(clientId);
        }

        public PlayerMatchData AddPlayer(ulong clientId, GameObject playerActor)
        {
            var state = playerActor.GetComponent<ActorState>();
            PlayerMatchData result;

            if (m_players.ContainsKey(clientId))
            {
                return m_players[clientId];
            }

            switch (m_matchData.MatchType)
            {
                case MatchType.Deathmatch:
                    result = this.AddPlayerToTeam(clientId, playerActor, Team.None);
                    break;
                case MatchType.TeamDeathmatch:
                case MatchType.CaptureTheFlag:
                    var autoJoinBlue = TeamBluePlayers.Count < TeamRedPlayers.Count;
                    result = this.AddPlayerToTeam(clientId, playerActor, autoJoinBlue ? Team.Blue : Team.Red);
                    break;
                default:
                    throw new InvalidOperationException("Invalid match type.");
            }

            m_players[clientId] = result;
            return result;
        }

        public void RemovePlayer(ulong clientId)
        {
            this.RemovePlayerFromTeam(clientId, Team.Blue);
            this.RemovePlayerFromTeam(clientId, Team.Red);
            this.RemovePlayerFromTeam(clientId, Team.None);

            m_players.Remove(clientId);
        }

        private void RemovePlayerFromTeam(ulong clientId, Team team)
        {
            if (m_matchData.Teams.ContainsKey(team))
            {
                var matchTeam = m_matchData.Teams[team];
                matchTeam.Players.Remove(clientId);
            }
        }

        private PlayerMatchData AddPlayerToTeam(ulong clientId, GameObject player, Team team)
        {
            if (clientId != player.GetComponent<ActorNetwork>().OwnerClientId)
            {
                throw new Exception("Client Id mismatch");
            }

            if (!m_matchData.Teams.ContainsKey(team))
            {
                throw new Exception("Team does not exist. Has this match manager been initialised?");
            }

            PlayerMatchData playerData = null;

            if (m_matchData.Teams[team].Players.ContainsKey(clientId))
            {
                NotificationService.Instance.Info($"Client {clientId}, player is already in team '{team}'. Updating player");
                var existing = m_matchData.Teams[team].Players[clientId];
                existing.Player = player;
                existing.ClientId = clientId;
                existing.Team = team;
                playerData = existing;
            }
            else
            {
                NotificationService.Instance.Info($"Client {clientId} is being added to team : '{team}'.");
                playerData = new PlayerMatchData { Player = player, Score = 0, ClientId = clientId, Team = team }; 
                m_matchData.Teams[team].Players.Add(clientId, playerData);
                this.OnPlayerAdded?.Invoke(this, new OnPlayerAddedArgs(playerData));
            }

            var state = player.GetComponent<ActorState>();

            if (state.Team != team)
            {
                state.Team = team;
                this.OnPlayerTeamSwitch?.Invoke(this, new OnPlayerSwitchTeamsArgs(playerData, team));
            }

            return playerData;
        }

        public List<PlayerMatchData> GetPayerMatchDataByTeam(Team team)
        {
            return m_matchData.Teams[team].Players.Values.ToList();
        }

        public PlayerMatchData GetPlayerMatchData(ulong clientId)
        {
            if (!m_players.ContainsKey(clientId))
            {
                return null;
            }

            return m_players[clientId];
        }

        public void AddTeamScore(Team team, int amount)
        {
            m_matchData.Teams[team].TeamScore += amount;
        }

        public void AddPlayerScore(GameObject player, int amount)
        {
            var clientId = player.GetComponent<ActorNetwork>().OwnerClientId;
            var team = player.GetComponent<ActorState>().Team;
            m_matchData.Teams[team].Players[clientId].Score += amount;
        }

        public TeamData GetTopTeamData()
        {
            var team = m_matchData.Teams.OrderByDescending(x => x.Value.TeamScore).First().Value;
            return team;
        }

        public PlayerMatchData GetTopPlayerMatchData(Team team)
        {
            var players = m_matchData.Teams[team].Players;

            if (!players.Any())
            {
                return null;
            }

            var player = players.OrderByDescending(x => x.Value.Score).FirstOrDefault();


            return player.Value;
        }

        private void MatchManager_OnMatchStateChanged(object sender, EventArgs e)
        {
            NotificationService.Instance.Info($"{m_matchData.MatchState}");
        }

        public void SetMatchData(MatchData matchData)
        {
            if (m_matchReady)
            {
                m_matchData.CurrentTime = matchData.CurrentTime;
                m_matchData.MatchState = matchData.MatchState;
                m_matchData.MatchType = matchData.MatchType;
                m_matchData.ScoreLimit = matchData.ScoreLimit;
                m_matchData.TimeLimit = matchData.TimeLimit;
                m_matchData.Teams = matchData.Teams;
            }
            else
            {
                this.InitialiseMatch(matchData);
            }
        }
    }
}