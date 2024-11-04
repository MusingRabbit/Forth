using Assets.Scripts;
using Assets.Scripts.Network;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Match
{
    public interface IReadonlyMatchData
    {
        float CurrentTime { get; }
        MatchState MatchState { get; }
        MatchType MatchType { get; }
        int ScoreLimit { get; }
        Dictionary<Team, IReadonlyTeamData> Teams { get; }
        TimeSpan TimeLimit { get; }

        MatchDataNet ToMatchDataNet();
        IReadonlyPayerMatchData GetTopPlayerByTeam(Team team);
    }
}