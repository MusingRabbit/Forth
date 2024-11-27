using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Match
{
    /// <summary>
    /// Interface for readonly player match data
    /// </summary>
    public interface IReadonlyPayerMatchData
    {
        GameObject Player { get; }
        int Score { get; }
        ulong ClientId { get; }

        PayerMatchDataNet ToPayerMatchDataNet();
    }
}