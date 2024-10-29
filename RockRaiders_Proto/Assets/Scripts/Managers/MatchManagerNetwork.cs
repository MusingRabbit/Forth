﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class PlayerDataNet : INetworkSerializable
    {
        public ulong PlayerNetworkObjectId;
        public int Score;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.PlayerNetworkObjectId);
            serializer.SerializeValue(ref this.Score);
        }
    }

    public class TeamData : INetworkSerializable
    {
        public Team Team;
        public int TeamScore;
        public string PlayersJson;

        public Dictionary<ulong, PlayerData> Players
        {
            get
            {
                return JsonConvert.DeserializeObject<Dictionary<ulong, PlayerData>>(this.PlayersJson);
            }
            set
            {
                this.PlayersJson = JsonConvert.SerializeObject(value);
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.Team);
            serializer.SerializeValue(ref this.TeamScore);
            serializer.SerializeValue(ref this.PlayersJson);
        }
    }

    public class MatchManagerNetwork : NetworkBehaviour
    {
        private NetworkVariable<TeamData> m_teamData;
        private NetworkVariable<PlayerDataNet> m_playerData;

        private MatchManager m_matchManager;

        public MatchManagerNetwork()
        {
            m_teamData = new NetworkVariable<TeamData>(writePerm: NetworkVariableWritePermission.Server);
            m_playerData = new NetworkVariable<PlayerDataNet>(writePerm: NetworkVariableWritePermission.Server);
        }

        private void Start()
        {
            m_matchManager = this.GetComponent<MatchManager>();
        }

        private void Update()
        {
            if (this.IsServer && this.IsOwner)
            {
                //Send match state
            }
            else
            {
                // Update match State
            }
        }
    }
}
