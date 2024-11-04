using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Match
{
    public interface IReadonlyPayerMatchData
    {
        GameObject Player { get; }
        int Score { get; }
        ulong ClientId { get; }

        PayerMatchDataNet ToPayerMatchDataNet();
    }
}