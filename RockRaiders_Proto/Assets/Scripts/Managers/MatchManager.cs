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
    /// <summary>
    /// Match manager
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton isntnace
        /// </summary>
        private static MatchManager _instance;
        public static MatchManager Instance
        {
            get
            {
                return _instance;
            }
        }


        /// <summary>
        /// Stores flag indicating whether match ready
        /// </summary>
        private bool m_matchReady;

        /// <summary>
        /// Store flag indicating whether manager is initialised
        /// </summary>
        private bool m_initalised;

        /// <summary>
        /// Match data
        /// </summary>
        private MatchData m_matchData;

        /// <summary>
        /// Player match data
        /// </summary>
        private Dictionary<ulong, PlayerMatchData> m_players;

        /// <summary>
        /// Blue team players
        /// </summary>
        private Dictionary<ulong, PlayerMatchData> TeamBluePlayers => m_matchData.Teams[Team.Blue].Players;

        /// <summary>
        /// Red team data 
        /// </summary>
        private Dictionary<ulong, PlayerMatchData> TeamRedPlayers => m_matchData.Teams[Team.Red].Players;

        /// <summary>
        /// Flag bases in match
        /// </summary>
        private List<FlagBase> m_flagBases;

        /// <summary>
        /// Fired when Initialise() is called
        /// </summary>
        public event EventHandler<EventArgs> OnInitialisation;

        /// <summary>
        /// Fired whenever the match state has changed
        /// </summary>
        public event EventHandler<EventArgs> OnMatchStateChanged;

        /// <summary>
        /// Fired whenever a player has been added to this match manager
        /// </summary>
        public event EventHandler<OnPlayerAddedArgs> OnPlayerAdded;

        /// <summary>
        /// Fired whenever a player switches team.
        /// </summary>
        public event EventHandler<OnPlayerSwitchTeamsArgs> OnPlayerTeamSwitch;

        /// <summary>
        /// Fired whenever a player has been removed.
        /// </summary>
        public event EventHandler<OnPlayerRemovedArgs> OnPlayerRemoved;

        /// <summary>
        /// Gets the flag indicating whether match is ready.
        /// </summary>
        public bool IsReady
        {
            get
            {
                return m_matchReady;
            }
        }

        /// <summary>
        /// Gets or sets whether match manager is currently in game.
        /// </summary>
        public bool InGame { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MatchManager()
        {
            m_matchData = new MatchData();
            
            m_players = new Dictionary<ulong, PlayerMatchData>();

            m_matchData.MatchState = MatchState.Ended;
            this.OnMatchStateChanged += this.MatchManager_OnMatchStateChanged;
        }

        /// <summary>
        /// Called on scene load
        /// </summary>
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

        /// <summary>
        /// Called before first frame in scene
        /// </summary>
        private void Start()
        {
            m_matchData.MatchType = MatchType.Deathmatch;
            m_matchData.MatchState = MatchState.Ended;
            m_matchData.ScoreLimit = 5;

            this.SubscribeToNotifications();
        }


        /// <summary>
        /// Called every frame
        ///     -> If pending start 
        ///         -> Run update for pending start state
        ///     -. If running
        ///         -> Run update for run state
        /// </summary>
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

        /// <summary>
        /// Subscribes to on player killed notifications
        /// </summary>
        private void SubscribeToNotifications()
        {
            NotificationService.Instance.OnPlayerKilled += this.Notification_OnPlayerKilled;
        }

        /// <summary>
        /// This is the update that is run when the match manager is pending update
        ///     -> Deathmatch : When there is more than one player -> Start the match
        ///     -. TeamDM / CTF : When each team has at least one player -> Start the match
        /// </summary>
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

        /// <summary>
        /// Performs this update when the match is in a run state.
        ///     -> Increments current time by delta time, and checks to see if time limit has been reached
        ///     -> Deathmatch : Gets the top scoring player, and checks if the score limit has been reached
        ///     -> CTF / TDM : Evaluates the scores of red and blue team and checks if the core limit has been reached
        ///     -> If score limit has been reached or time limit has been reached
        ///         -> End the match
        /// </summary>
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

        /// <summary>
        /// This method is run whenever a played killed event has been triggered.
        ///     -> Checks to see if the player has a killer
        ///         -> if TDM -> Checks if the killer was on the same team, modifies score accordingly, add/remove point from team score.
        ///         -> Adds a point to the killers score.
        ///     -> If not - the player killed themselves
        ///         -> Subtract a point from player score, and team score if matchtype id TDM
        /// </summary>
        /// <param name="sender">the object that triggered the event</param>
        /// <param name="e">Event arguments <see cref="OnNotificationEventArgs"/></param>
        private void Notification_OnPlayerKilled(object sender, OnNotificationEventArgs e)
        {
            var killedData = e.Data.GetData<PlayerKilledData>();

            if (killedData.Killer != null)  // If the player has a killer
            {
                var killerState = killedData.Killer.GetComponent<ActorState>();
                var killedState = killedData.Killed.GetComponent<ActorState>();

                switch (m_matchData.MatchType)
                {
                    case MatchType.TeamDeathmatch:
                        var score = killedState.Team != killerState.Team ? 1 : -1; // Teamkill check

                        this.AddTeamScore(killerState.Team, score);
                        break;
                }


                this.AddPlayerScore(killedData.Killer, 1);
            }
            else // Player killed themselves
            {
                var killedState = killedData.Killed.GetComponent<ActorState>();

                switch (m_matchData.MatchType)
                {
                    case MatchType.TeamDeathmatch:
                        this.AddTeamScore(killedState.Team, -1);
                        break;
                }
            }
        }

        /// <summary>
        /// Initialises the match manager
        /// </summary>
        public void Initialise()
        {
            this.OnInitialisation?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets match data and initialises match
        ///     -> Sets Matchtype, Score limit and time limit from match settings
        ///     -> Assigns match state from matchState parameter
        ///     -> Initialises match with matach data
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="matchState"></param>
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

        /// <summary>
        /// Initialises match
        ///     -> Copies match data
        ///     -> if DM : Creates a team 'None' and adds 
        ///     -> else : Creates teams 'Blue' and 'Red'
        ///         -> If CTF
        ///             -> Gets all flag bases within the current scene and subscribes to their 'flag captured' event
        ///     -> Adds all players to the player dictionary
        ///     -> Sets match ready to be true
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Called whenever a flag captured event has been raised by a flag base.
        /// -> Determines which team to allocate a point, and adds that point to the teams score.
        /// </summary>
        /// <param name="sender">Sender (flag base)</param>
        /// <param name="e">Event args</param>
        private void FlagBase_FlagCaptured(object sender, EventArgs e)
        {
            var fbase = (FlagBase)sender;
            var team = fbase.Team == Team.Blue ? Team.Red : fbase.Team == Team.Red ? Team.Blue : Team.None;
            this.AddTeamScore(team, 1);
        }

        /// <summary>
        /// Gets all flag bases in the specified scene
        /// </summary>
        /// <param name="scene">Scene to query</param>
        /// <returns>A list of flag bases</returns>
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

        /// <summary>
        /// Returns a readonly version of match data
        /// </summary>
        /// <returns>Match data</returns>
        public IReadonlyMatchData GetMatchData()
        {
            return m_matchData.ToReadonlyMatchData();
        }

        /// <summary>
        /// Returns true if the specified client Id exists within this match managers' player dictionary.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>Is registered? (true/false)</returns>
        public bool PlayerExists(ulong clientId)
        {
            return m_players.ContainsKey(clientId);
        }

        /// <summary>
        /// Adds or gets player match data for this match manager
        ///     -> Returns player match data if it already exists
        ///     -> If Deathmatch : Adds player to team 'None'
        ///     -> Else : 
        ///         -> Adds this player to the team with the least number of players
        ///     -> Returns player match data
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="playerActor">actor</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">player actor cannot be null</exception>
        /// <exception cref="InvalidOperationException">Invalid match type in match data</exception>
        public PlayerMatchData AddPlayer(ulong clientId, GameObject playerActor)
        {
            if (playerActor == null)
            {
                throw new ArgumentNullException(nameof(playerActor));
            }

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

        /// <summary>
        /// Removes player from the current match, and invokes an event for all subscribers
        /// </summary>
        /// <param name="clientId">client id</param>
        public void RemovePlayer(ulong clientId)
        {
            this.RemovePlayerFromTeam(clientId, Team.Blue);
            this.RemovePlayerFromTeam(clientId, Team.Red);
            this.RemovePlayerFromTeam(clientId, Team.None);

            var data = m_players[clientId];

            m_players.Remove(clientId);

            this.OnPlayerRemoved?.Invoke(this, new OnPlayerRemovedArgs(data));
        }

        /// <summary>
        /// Removes player from the specified team
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="team">Team to remove the player from</param>
        private void RemovePlayerFromTeam(ulong clientId, Team team)
        {
            if (m_matchData.Teams.ContainsKey(team))
            {
                var matchTeam = m_matchData.Teams[team];
                matchTeam.Players.Remove(clientId);
            }
        }

        /// <summary>
        /// Adds player to the specified team. 
        ///     -> Checks if client id is valid for the given player object
        ///     -> Checks whether the team specified exists within the match manager
        ///     -> Checks if the player already exists within the team
        ///         -> Updates the player actor if so
        ///     -> Else
        ///         -> Adds the player to the specified team
        ///     -> Invokes an 'on team switch' event to notify any and all subscribers
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="player">Actor entity / Player game object</param>
        /// <param name="team">Team to join</param>
        /// <returns>Player match data <see cref="PlayerMatchData"/></returns>
        /// <exception cref="Exception">Invalid client id / team does not exist</exception>
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

        /// <summary>
        /// Gets player match data for all players by team
        /// </summary>
        /// <param name="team">The team of which to get the player match data</param>
        /// <returns>A list of player match data <see cref="PlayerMatchData"/></returns>
        public List<PlayerMatchData> GetPayerMatchDataByTeam(Team team)
        {
            return m_matchData.Teams[team].Players.Values.ToList();
        }

        /// <summary>
        /// Gets player match data for a specific player
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>Player match data</returns>
        public PlayerMatchData GetPlayerMatchData(ulong clientId)
        {
            if (!m_players.ContainsKey(clientId))
            {
                return null;
            }

            return m_players[clientId];
        }

        /// <summary>
        /// Adds points to the specified teams score.
        /// </summary>
        /// <param name="team">Team of which to allocate points</param>
        /// <param name="amount">The amount of points to add</param>
        public void AddTeamScore(Team team, int amount)
        {
            m_matchData.Teams[team].TeamScore += amount;
        }

        /// <summary>
        /// Adds points to the specified players score.
        /// </summary>
        /// <param name="player">Player of which to allocate points</param>
        /// <param name="amount">The amount of points to add</param>
        public void AddPlayerScore(GameObject player, int amount)
        {
            var clientId = player.GetComponent<ActorNetwork>().OwnerClientId;
            var team = player.GetComponent<ActorState>().Team;
            m_matchData.Teams[team].Players[clientId].Score += amount;
        }

        /// <summary>
        /// Gets the team data for the team that is currently winning.
        /// </summary>
        /// <returns>Team data</returns>
        public TeamData GetTopTeamData()
        {
            var team = m_matchData.Teams.OrderByDescending(x => x.Value.TeamScore).First().Value;
            return team;
        }

        /// <summary>
        /// Gets the player match data for the player that is currently winning
        /// </summary>
        /// <param name="team">Player team</param>
        /// <returns>Player match data</returns>
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

        /// <summary>
        /// Fired when match state has been changed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void MatchManager_OnMatchStateChanged(object sender, EventArgs e)
        {
            NotificationService.Instance.Info($"{m_matchData.MatchState}");
        }

        /// <summary>
        /// Sets the match data for the current match
        /// </summary>
        /// <param name="matchData">Match data</param>
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