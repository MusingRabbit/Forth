using Assets.Scripts;
using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Match
{
    public class MatchData
    {
        public MatchState MatchState { get; set; }
        public MatchType MatchType { get; set; }
        public int ScoreLimit { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public float CurrentTime { get; set; }
        public Dictionary<Team, TeamData> Teams { get; set; }

        public MatchData()
        {
            this.Teams = new Dictionary<Team, TeamData>();
        }

        public IReadonlyMatchData ToReadonlyMatchData()
        {
            return new ReadonlyMatchData(this);
        }
    }

    public class ReadonlyMatchData : IReadonlyMatchData
    {
        public MatchType MatchType { get; }
        public MatchState MatchState { get; }
        public int ScoreLimit { get; }
        public TimeSpan TimeLimit { get; }
        public float CurrentTime { get; }
        public Dictionary<Team, IReadonlyTeamData> Teams { get; }

        
        public ReadonlyMatchData(MatchData matchData)
        {
            this.MatchState = matchData.MatchState;
            this.MatchType = matchData.MatchType;
            this.ScoreLimit = matchData.ScoreLimit;
            this.TimeLimit = matchData.TimeLimit;
            this.CurrentTime = matchData.CurrentTime;

            this.Teams = new Dictionary<Team, IReadonlyTeamData>();

            foreach (var kvp in matchData.Teams)
            {
                this.Teams.Add(kvp.Key, kvp.Value.ToReadonlyTeamData());
            }
        }

        public MatchDataNet ToMatchDataNet()
        {
            var result = new MatchDataNet();

            result.CurrentTime = this.CurrentTime;
            result.MatchState = this.MatchState;
            result.ScoreLimit = this.ScoreLimit;
            result.MatchType = this.MatchType;
            result.TimeLimit = (float)this.TimeLimit.TotalSeconds;

            result.Teams = this.Teams.Select(x => x.Value.ToTeamDataNet()).ToArray();

            return result;
        }

        public IReadonlyPayerMatchData GetTopPlayerByTeam(Team team)
        {
            var result = this.Teams[team].Players.OrderByDescending(x => x.Value.Score).Select(x => x.Value).FirstOrDefault();
            return result;
        }
    }
}