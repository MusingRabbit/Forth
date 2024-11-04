using Assets.Scripts.Network;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Match
{
    public class PlayerMatchData
    {
        public ulong ClientId { get; set; }
        public GameObject Player { get; set; }
        public Team Team { get; set; }
        public int Score { get; set; }
    }

    public class ReadonlyPlayerMatchData : IReadonlyPayerMatchData
    {
        public GameObject Player { get; }
        public int Score { get; }
        public ulong ClientId { get; }

        public ReadonlyPlayerMatchData(PlayerMatchData value)
        {
            this.Player = value.Player;
            this.Score = value.Score;
            this.ClientId = value.ClientId;
        }

        public PayerMatchDataNet ToPayerMatchDataNet()
        {
            var result = new PayerMatchDataNet();

            result.ClientId = this.ClientId;
            result.Score = this.Score;

            if (this.Player != null)
            {
                result.PlayerNetworkObjectId = this.Player.GetComponent<NetworkObject>().NetworkObjectId;
            }

            return result;
        }
    }
}