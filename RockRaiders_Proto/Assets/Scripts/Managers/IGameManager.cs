using Assets.Scripts.UI.Models;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public interface IGameManager
    {
        /// <summary>
        /// Gets a flag indicating whether the local player is awaiting respawn.
        /// </summary>
        bool LocalPlayerAwaitingRespawn { get; }

        /// <summary>
        /// Gets a flag indicating whether the local player has entered the in-game menu.
        /// </summary>
        bool PlayerPaused { get; }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        SettingsModel Settings { get; }

        /// <summary>
        /// Unhooks events and deregisters player for provided client Id
        /// </summary>
        /// <param name="clientId"></param>
        void UnhookPlayerActor(ulong clientId);
        
        /// <summary>
        /// Checks to see whether player is contained within the game managers' player dictionary.
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <returns>Is registered? (true/false)</returns>
        bool IsPlayerRegistered(ulong clientId);

        /// <summary>
        /// Launches the game   
        /// </summary>
        void LaunchGame();

        /// <summary>
        /// Loads the splash screen
        /// </summary>
        void LoadSplashScreen();

        /// <summary>
        /// Used to notify the game manager that the local player has closed the pause menu
        /// </summary>
        void NotifyPauseMenuClosed();

        /// <summary>
        /// Shutsdown the game, and loads the spashscreen.
        /// </summary>
        void QuitGame();

        /// <summary>
        /// Registers player with the game manager
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="playerActor">player actor entity</param>
        void RegisterPlayer(ulong clientId, GameObject playerActor);
        
        /// <summary>
        /// Respawns the local player
        /// </summary>
        void RespawnLocalPlayer();
    }
}