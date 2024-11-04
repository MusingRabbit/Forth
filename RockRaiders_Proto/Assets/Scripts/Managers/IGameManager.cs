using Assets.Scripts.UI.Models;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public interface IGameManager
    {
        bool InGame { get; }
        bool LocalPlayerAwaitingRespawn { get; }
        bool PlayerPaused { get; }
        SettingsModel Settings { get; }

        void DetatchPlayerActor(ulong clientId);
        bool IsPlayerRegistered(ulong clientId);
        void LaunchGame();
        void LoadSplashScreen();
        void NotifyPauseMenuClosed();
        void QuitGame();
        void RegisterPlayer(ulong clientId, GameObject playerActor);
        void RespawnPlayer();
    }
}