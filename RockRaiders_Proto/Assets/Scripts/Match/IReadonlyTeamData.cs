using Assets.Scripts;
using Assets.Scripts.Network;
using System.Collections.Generic;
namespace Assets.Scripts.Match
{
    public interface IReadonlyTeamData
    {
        public Team Team { get; }
        public int TeamScore { get; }
        public Dictionary<ulong, IReadonlyPayerMatchData> Players { get; }

        TeamDataNet ToTeamDataNet();
    }
}