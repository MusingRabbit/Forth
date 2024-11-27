using Assets.Scripts.Actor;
using Assets.Scripts.Network;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Match
{
    /// <summary>
    /// Player Match Data
    /// </summary>
    public class PlayerMatchData
    {
        public ulong ClientId { get; set; }
        public GameObject Player { get; set; }
        public Team Team { get; set; }
        public int Score { get; set; }
    }

    /// <summary>
    /// Readonly variant of player match data
    /// </summary>
    public class ReadonlyPlayerMatchData : IReadonlyPayerMatchData
    {
        public GameObject Player { get; protected set; }
        public int Score { get; protected set; }
        public ulong ClientId { get; protected set; }

        public ReadonlyPlayerMatchData(PlayerMatchData value)
        {
            this.Player = value.Player;
            this.Score = value.Score;
            this.ClientId = value.ClientId;
        }

        /// <summary>
        /// Converts to network data
        /// </summary>
        /// <returns>Player match data network object</returns>
        public PayerMatchDataNet ToPayerMatchDataNet()
        {
            var result = new PayerMatchDataNet();

            result.ClientId = this.ClientId;
            result.Score = this.Score;

            if (this.Player != null)
            {
                result.PlayerNetworkObjectId = this.Player.GetComponent<NetworkObject>().NetworkObjectId;
                result.Team = this.Player.GetComponent<ActorState>().Team;
            }

            return result;
        }
    }
}