using Assets.Scripts.Match;
using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Assets.Scripts.Util
{
    public static class MatchDataExtensions
    {
        public static MatchData ToMatchData(this MatchDataNet netData)
        {
            var result = new MatchData();
            result.CurrentTime = netData.CurrentTime;
            result.MatchState = netData.MatchState;
            result.ScoreLimit = netData.ScoreLimit;
            result.MatchType = netData.MatchType;
            result.TimeLimit = TimeSpan.FromSeconds(netData.TimeLimit);
            result.Teams = new Dictionary<Team, TeamData>();

            foreach(var team in netData.Teams)
            {
                var data = team.ToTeamData();

                if (!result.Teams.ContainsKey(team.Team))
                {
                    result.Teams.Add(team.Team, data);
                }
                else
                {
                    var currTeam = result.Teams[team.Team];
                    currTeam.Players = data.Players;
                    currTeam.Team = data.Team;
                    currTeam.TeamScore = data.TeamScore;
                }
            }

            return result;
        }

        public static MatchDataNet ToMatchDataNet(this MatchData matchData)
        {
            var result = new MatchDataNet();

            result.CurrentTime = matchData.CurrentTime;
            result.MatchState = matchData.MatchState;
            result.ScoreLimit = matchData.ScoreLimit;
            result.MatchType = matchData.MatchType;
            result.TimeLimit = (float)matchData.TimeLimit.TotalSeconds;

            result.Teams = matchData.Teams.Select(x => x.Value.ToTeamDataNet()).ToArray();

            return result;
        }
    }
}
