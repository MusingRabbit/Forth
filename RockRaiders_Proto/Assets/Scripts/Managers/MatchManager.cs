using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class PlayerData
{
    public GameObject Player { get; set; }
    public int Score { get; set; }
}

public class TeamData
{
    public Team Team { get; set; }
    public int TeamScore { get; set; }
    public Dictionary<ulong, PlayerData> Players { get; set; }
    
}

public class MatchManager : NetworkBehaviour
{
    [SerializeField]
    private MatchType m_matchType;

    public MatchType MatchType
    {
        get
        {
            return m_matchType;
        }
        set
        {
            m_matchType = value;
        }
    }

    private Dictionary<Team, TeamData> m_teamDictionary;
    private Dictionary<ulong, PlayerData> m_teamBluePlayers;
    private Dictionary<ulong, PlayerData> m_teamRedPlayers;

    public MatchManager()
    {
        m_matchType = MatchType.Deathmatch;
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (this.IsServer)
        {
            this.InitialiseTeamDictionary();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (this.IsServer)
        {

        }
    }

    private void SubscribeToNotifications()
    {
        NotificationService.Instance.OnPlayerKilled += this.Notification_OnPlayerKilled;
    }

    private void Notification_OnPlayerKilled(object sender, Assets.Scripts.Events.OnNotificationEventArgs e)
    {
        if (this.IsServer)
        {
            var data = e.Data.GetData<PlayerKilledData>();
            this.AddPlayerScore(data.Killer, 1);
        }
    }

    private void InitialiseTeamDictionary()
    {
        m_teamDictionary = new Dictionary<Team, TeamData>();

        switch (m_matchType)
        {
            case MatchType.Deathmatch:
                m_teamDictionary.Add(Team.None, new TeamData());
                break;
            case MatchType.TeamDeathmatch:
            case MatchType.CaptureTheFlag:
                m_teamDictionary.Add(Team.Red, new TeamData());
                m_teamDictionary.Add(Team.Blue, new TeamData());

                m_teamBluePlayers = m_teamDictionary[Team.Blue].Players;
                m_teamRedPlayers = m_teamDictionary[Team.Red].Players;
                break;
        }
    }

    public void RegisterPlayer(ulong clientId, GameObject playerActor)
    {
        var state = playerActor.GetComponent<ActorState>();

        switch (m_matchType)
        {
            case MatchType.Deathmatch:
                this.AddPlayerToTeam(clientId, playerActor, Team.None);
                break;
            case MatchType.TeamDeathmatch:
            case MatchType.CaptureTheFlag:
                var autoJoinBlue = m_teamBluePlayers.Count < m_teamRedPlayers.Count;
                this.AddPlayerToTeam(clientId, playerActor, autoJoinBlue ? Team.Blue : Team.Red);
                break;
        }
    }

    private void AddPlayerToTeam(ulong clientId, GameObject player, Team team)
    {
        if (m_teamDictionary[team].Players.ContainsKey(clientId))
        {
            NotificationService.Instance.Info($" Client Id : {clientId}, player is already in team '{team}'. Updating player");
            var existing = m_teamDictionary[team].Players[clientId];
            existing.Player = player;
        }
        else
        {
            m_teamDictionary[team].Players.Add(clientId, new PlayerData { Player = player, Score = 0 });
        }

        if (clientId != player.GetComponent<ActorNetwork>().OwnerClientId)
        {

        }

        var state = player.GetComponent<ActorState>();
        state.Team = team;
    }

    public List<PlayerData> GetPlayerDataByTeam(Team team)
    {
        return m_teamDictionary[team].Players.Values.ToList();
    }

    public void AddTeamScore(Team team, int amount)
    {
        m_teamDictionary[team].TeamScore += amount;
    }

    public void AddPlayerScore(GameObject player, int amount)
    {
        var clientId = player.GetComponent<ActorNetwork>().OwnerClientId;
        var team = player.GetComponent<ActorState>().Team;
        m_teamDictionary[team].Players[clientId].Score += amount;
    }
}