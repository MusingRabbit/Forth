using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Match;
using Assets.Scripts.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class PayerMatchDataNet : INetworkSerializable, IEquatable<PayerMatchDataNet>
    {
        public ulong ClientId;
        public ulong PlayerNetworkObjectId;
        public int Score;
        public Team Team;
        
        public bool Equals(PayerMatchDataNet other)
        {
            return this.PlayerNetworkObjectId == other.PlayerNetworkObjectId
                && this.Score == other.Score
                && this.Team == other.Team
                && this.ClientId == other.ClientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerNetworkObjectId);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Team);
        }
    }

    public class TeamDataNet : INetworkSerializable, IEquatable<TeamDataNet>
    {
        public Team Team;
        public int TeamScore;
        public PayerMatchDataNet[] Players;

        public bool Equals(TeamDataNet other)
        {
            return this.Team == other.Team && this.TeamScore == other.TeamScore && this.Players.Equals(other.Players);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.Team);
            serializer.SerializeValue(ref this.TeamScore);
            serializer.SerializeValue(ref this.Players);
        }
    }

    public class MatchDataNet : INetworkSerializable, IEquatable<MatchDataNet>
    {
        public MatchState MatchState;
        public MatchType MatchType;
        public int ScoreLimit;
        public float TimeLimit;
        public float CurrentTime;

        public TeamDataNet[] Teams;

        public bool Equals(MatchDataNet other)
        {
            return this.MatchState == other.MatchState
                && this.MatchType == other.MatchType
                && this.ScoreLimit == other.ScoreLimit
                && this.TimeLimit == other.TimeLimit
                && this.CurrentTime == other.CurrentTime
                && this.Teams.Equals(other.Teams);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.MatchState);
            serializer.SerializeValue(ref this.MatchType);
            serializer.SerializeValue(ref this.ScoreLimit);
            serializer.SerializeValue(ref this.TimeLimit);
            serializer.SerializeValue(ref this.CurrentTime);
            serializer.SerializeValue(ref this.Teams);
        }
    }

    public class MatchManagerNetwork : NetworkBehaviour
    {
        private MatchManager m_matchManager;
        private Timer m_updateTimer;

        public MatchManagerNetwork()
        {
            m_updateTimer = new Timer(TimeSpan.FromSeconds(3));
            m_updateTimer.AutoReset = false;
            m_updateTimer.OnTimerElapsed += UpdateTimer_OnTimerElapsed;
        }


        private void Start()
        {
            m_matchManager = this.GetComponent<MatchManager>();
            m_matchManager.OnPlayerAdded += this.MatchManager_OnPlayerAdded;
            m_matchManager.OnPlayerTeamSwitch += this.MatchManager_OnPlayerTeamSwitch;

            m_updateTimer.Start();

            this.NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

        }

        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            this.Initialise();
        }

        public void Initialise()
        {
            if (!this.IsServer && this.NetworkManager != null)
            {
                this.SendMatchDataToClient(this.NetworkManager.LocalClientId);
            }
        }

        private void Awake()
        {

        }

        private void MatchManager_OnPlayerTeamSwitch(object sender, OnPlayerSwitchTeamsArgs e)
        {
        }

        private void MatchManager_OnPlayerAdded(object sender, OnPlayerAddedArgs e)
        {
            this.SendMatchDataToClient(e.PlayerData.ClientId);
        }

        private List<PayerMatchDataNet> GetPayerMatchDataNetByTeam(Team team)
        {
            var playerData = m_matchManager.GetPayerMatchDataByTeam(team).Select(x => x.ToPayerMatchDataNet());
            return playerData.ToList();
        }

        private void Update()
        {
            if (m_matchManager.GameManager?.InGame ?? false)
            {
                m_updateTimer.Tick();
            }
            else
            {
                m_updateTimer.ResetTimer();
            }
        }

        public void SendMatchDataToClient(ulong clientId)
        {
            if (!this.IsServer)
            {
                return;
            }

            var rpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };

            var data = m_matchManager.GetMatchData().ToMatchDataNet();

            this.SendMatchDataClientRpc(data, rpcParams);
        }

        [ClientRpc]
        private void SendMatchDataClientRpc(MatchDataNet data, ClientRpcParams clientRpcParams = default)
        {
            MatchData matchData = data.ToMatchData();

            m_matchManager.SetMatchData(matchData);
        }

        private void UpdateTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_updateTimer.ResetTimer();
            if (this.IsServer)
            {
                var data = m_matchManager.GetMatchData().ToMatchDataNet();
                this.SendMatchDataClientRpc(data);
            }

        }

        public override void OnNetworkSpawn()
        {

        }


        private void HandleNetData()
        {
            if (IsServer)
            {

            }
            else
            {
                // Retreive match State
            }
        }
    }
}
