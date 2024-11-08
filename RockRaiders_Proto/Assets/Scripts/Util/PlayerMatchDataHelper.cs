using Assets.Scripts.Match;
using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class PlayerMatchDataHelper
    {
        public static PayerMatchDataNet ToPayerMatchDataNet(this PlayerMatchData playerData)
        {
            PayerMatchDataNet result = new PayerMatchDataNet();

            if (playerData.Player != null)
            {
                result.PlayerNetworkObjectId = playerData.Player.GetComponent<NetworkObject>().NetworkObjectId;
            }

            result.Score = playerData.Score;
            result.ClientId = playerData.ClientId;

            return result;
        }

        public static PlayerMatchData ToPayerMatchData(this PayerMatchDataNet playerDataNet)
        {
            PlayerMatchData result = new PlayerMatchData();

            if (playerDataNet.PlayerNetworkObjectId > 0)
            {
                result.Player = NetworkHelper.GetPlayerObjectByNetworkObjectId(playerDataNet.PlayerNetworkObjectId);
            }
            
            result.Score = playerDataNet.Score;
            result.ClientId = playerDataNet.ClientId;
            result.Team = playerDataNet.Team;

            return result;
        }
    }
}
