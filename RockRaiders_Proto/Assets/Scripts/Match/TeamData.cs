using Assets.Scripts;
using Assets.Scripts.Match;
using Assets.Scripts.Network;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Match
{
    /// <summary>
    /// Team data - Stores all match data for a given team.
    /// </summary>
    public class TeamData
    {
        public Team Team { get; set; }
        public int TeamScore { get; set; }
        public Dictionary<ulong, PlayerMatchData> Players { get; set; }

        public TeamData()
        {
            this.Players = new Dictionary<ulong, PlayerMatchData>();
        }

        public TeamData(Team team)
        {
            this.Team = team;
            this.Players = new Dictionary<ulong, PlayerMatchData>();
        }

        public ReadonlyTeamData ToReadonlyTeamData()
        {
            return new ReadonlyTeamData(this);
        }
    }

    /// <summary>
    /// Readonly variant of team data. Can be read - but not modified.
    /// </summary>
    public class ReadonlyTeamData : IReadonlyTeamData
    {
        public Team Team { get; private set; }
        public int TeamScore { get; private set; }
        public Dictionary<ulong, IReadonlyPayerMatchData> Players { get; private set; }

        public ReadonlyTeamData(TeamData teamData)
        {
            this.Team = teamData.Team;
            this.TeamScore = teamData.TeamScore;
            this.Players = new Dictionary<ulong, IReadonlyPayerMatchData>();

            foreach (var kvp in teamData.Players)
            {
                this.Players.Add(kvp.Key, new ReadonlyPlayerMatchData(kvp.Value));
            }
        }

        /// <summary>
        /// Converts to network data
        /// </summary>
        /// <returns>Team net data object</returns>
        public TeamDataNet ToTeamDataNet()
        {
            TeamDataNet result = new TeamDataNet();

            result.Team = this.Team;
            result.TeamScore = this.TeamScore;
            result.Players = this.Players.Select(x => x.Value?.ToPayerMatchDataNet()).ToArray();

            return result;
        }
    }
}