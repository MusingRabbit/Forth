using Assets.Scripts.Match;
using Assets.Scripts.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Util
{
    public static class TeamDataExtensions
    {
        public static TeamDataNet ToTeamDataNet(this TeamData teamData)
        {
            TeamDataNet result = new TeamDataNet();

            result.Team = teamData.Team;
            result.TeamScore = teamData.TeamScore;
            result.Players = teamData.Players.Select(x => x.Value.ToPayerMatchDataNet()).ToArray();

            return result;
        }

        public static TeamData ToTeamData(this TeamDataNet netData)
        {
            TeamData result = new TeamData();

            var playersData = new Dictionary<ulong, PlayerMatchData>(netData.Players.Count());

            foreach (var player in netData.Players)
            {
                if (!playersData.TryAdd(player.ClientId, player.ToPayerMatchData()))
                {
                    playersData[player.ClientId] = player.ToPayerMatchData();
                }
            }

            result.Players = playersData;
            result.TeamScore = netData.TeamScore;
            result.Team = netData.Team;

            return result;
        }
    }
}
