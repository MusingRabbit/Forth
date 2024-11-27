using Assets.Scripts.Actor;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.Managers;
using Assets.Scripts.Network;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// Responsible for managing the game
    /// </summary>
    public class GameManager : NetworkBehaviour, IGameManager
    {
        /// <summary>
        /// Singleton / static instance variable
        /// </summary>
        private static GameManager _instance;

        /// <summary>
        /// Getter for the singleton pattern.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                return _instance;
            }
        }


        /// <summary>
        /// Flag indicating whether the current (local) player has entered into the pause menu.
        /// </summary>
        private bool m_playerPaused;

        /// <summary>
        /// Flag indicating whether the (local) layer is awaiting respawn.
        /// </summary>
        private bool m_localPlayerAwaitingRespawn;

        /// <summary>
        /// Flag indicating whether the local machine is a client connecting to remote host.
        /// </summary>
        private bool m_clientConnecting;

        /// <summary>
        /// Stores a dictionary of current players
        /// </summary>
        private Dictionary<ulong, GameObject> m_players;

        /// <summary>
        /// Stores a reference to the current local player
        /// </summary>
        private GameObject m_localPlayer;

        /// <summary>
        /// Stores a reference to the settings repository
        /// </summary>
        private SettingsRepository m_settingsRepo;

        /// <summary>
        /// Gets whether the player is in the in-game menu.
        /// </summary>
        public bool PlayerPaused
        {
            get
            {
                return m_playerPaused;
            }
        }

        /// <summary>
        /// Gets whether the local player is awaiting respawn.
        /// </summary>
        public bool LocalPlayerAwaitingRespawn
        {
            get
            {
                return m_localPlayerAwaitingRespawn;
            }
        }

        /// <summary>
        /// Stores a reference to the network manager
        /// </summary>
        [SerializeField]
        private NetworkManager m_netManager;

        /// <summary>
        /// Stores a reference to the input manager / control management
        /// </summary>
        [SerializeField]
        private InputManager m_inputManager;

        /// <summary>
        /// Stores a reference to the actor spawn manager
        /// </summary>
        [SerializeField]
        private ActorSpawnManager m_spawnManager;

        /// <summary>
        /// Stores a reference to the weapon spawn manager.
        /// </summary>
        private WeaponSpawnManager m_weaponSpawnManager;

        /// <summary>
        /// Stores a reference to the audio manager
        /// </summary>
        [SerializeField]
        private AudioManager m_audioManager;

        /// <summary>
        /// Stores a reference to the match manager
        /// </summary>
        [SerializeField]
        private MatchManager m_matchManager;

        /// <summary>
        /// Stores the current gamestate.
        /// </summary>
        private GameState m_state;

        /// <summary>
        /// Stores a reference to the settings model.
        /// </summary>
        private SettingsModel m_settings;

        /// <summary>
        /// Stores a reference the unity transport, used for storing connection data
        /// </summary>
        private UnityTransport m_unityTransport;

        /// <summary>
        /// Gets the settings model stored by this game manager
        /// </summary>
        public SettingsModel Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameManager()
        {
            m_state = GameState.MainMenu;
            m_players = new Dictionary<ulong, GameObject>();

            m_settingsRepo = new SettingsRepository();
        }

        /// <summary>
        /// Called on load.
        /// </summary>
        private void Awake()
        {
            if (_instance == null)
            {
                this.LoadSettings();                                // Retreived game settings from disk / perm storage
                _instance = this;

                GameObject.DontDestroyOnLoad(base.gameObject);      // Sets 'dont destroy on load flag' such that this instance persists between scenes
                this.ClearWeaponSpawnManager();
            }
            else
            {
                GameObject.Destroy(base.gameObject);                // Destroys any additional instances of GameManager
            }
        }

        /// <summary>
        /// Called before the first frame
        /// </summary>
        private void Start()
        {
            m_unityTransport = m_netManager.GetComponent<UnityTransport>();
            m_netManager.OnConnectionEvent += this.NetManager_OnConnectionEvent;

            m_spawnManager.OnActorSpawn += this.SpawnManager_OnActorSpawned;
            m_spawnManager.OnActorDespawn += this.SpawnManager_OnActorDespawned;

            m_matchManager.Initialise();

            m_matchManager.OnMatchStateChanged += MatchManager_OnMatchStateChanged;
            m_matchManager.OnPlayerTeamSwitch += MatchManager_OnPlayerTeamSwitch;

            m_netManager.ConnectionApprovalCallback += this.Netmanager_OnConnectionApproval;
            m_netManager.OnClientConnectedCallback += this.Netmanager_OnClientConnectedCallback;
            m_netManager.OnClientDisconnectCallback += this.NetManager_OnClientDisconnectCallback;


        }

        /// <summary>
        /// Called every frame.
        /// -> If has input manager
        ///     -> Checks whether player has pressed the pause key
        ///         -> Disables input manager input
        ///         -> Unlocks mouse
        ///     -> Checks whether local player is awaiting respawn
        ///         -> Checks if trigger has been pressed
        ///             -> Respawns local player
        ///             -> Resets 'local player awaiting respawn' flag
        /// -> Update whether match manager is in game or not.
        /// </summary>
        private void Update()
        {
            if (m_inputManager.Controller != null)
            {
                if (m_inputManager.Controller.GetActionState(Input.ControllerActions.Pause) == Input.ActionState.Active)
                {
                    m_inputManager.enabled = false;
                    m_playerPaused = true;
                    this.UnlockMouse();
                    m_inputManager.Controller.SetActionState(ControllerActions.Pause, ActionState.InActive);
                }

                if (this.LocalPlayerAwaitingRespawn)
                {
                    if (m_inputManager.Controller.GetActionState(Input.ControllerActions.Trigger) == ActionState.Active)
                    {
                        m_spawnManager.RespawnLocalPlayer();
                        m_localPlayerAwaitingRespawn = false;
                    }
                }
            }

            m_matchManager.InGame = m_state == GameState.InGame;
        }

        /// <summary>
        /// Loads the settings from the settings repository and applies them.
        /// </summary>
        private void LoadSettings()
        {
            m_settings = m_settingsRepo.GetSettingsModel();
            m_audioManager.Settings = m_settings;
            m_inputManager.Settings = m_settings;

            Assets.Scripts.UI.Settings.SetScreenResolution(m_settings.Game.Resolution, m_settings.Game.FullScreen);
        }

        /// <summary>
        /// Gets the weapon spawn manager for the currently active scene.
        /// </summary>
        /// <returns>Weapon spawn manager, null if not found</returns>
        private WeaponSpawnManager GetWeaponSpawnManager()
        {
            WeaponSpawnManager result = m_weaponSpawnManager;

            if (result == null)
            {
                var spawnManagerObj = SceneManager.GetActiveScene().GetRootGameObjects().SingleOrDefault(x => x.name == "WeaponSpawnManager");

                if (spawnManagerObj == null)
                {
                    return null;
                }

                result = spawnManagerObj.GetComponent<WeaponSpawnManager>();
                m_weaponSpawnManager = result;
            }

            return result;
        }

        /// <summary>
        /// Nulls the local weapon spawn manager so it can be retreived from the scene next time it's retreived.
        /// </summary>
        private void ClearWeaponSpawnManager()
        {
            m_weaponSpawnManager = null;
        }

        /// <summary>
        /// Shutsdown the nwtork connection and loads the splashcreen scene.
        /// </summary>
        public void QuitGame()
        {
            this.NotifyPauseMenuClosed();
            m_netManager.Shutdown();
            this.LoadSplashScreen();
        }

        /// <summary>
        /// Called when pause menu has been closed by the player. Re-enables user inputes such that they game may be played again. 
        /// </summary>
        public void NotifyPauseMenuClosed()
        {
            m_inputManager.enabled = true;
            m_playerPaused = false;
            this.LockMouse();
            m_inputManager.Controller.SetActionState(ControllerActions.Pause, ActionState.InActive);
        }

        /// <summary>
        /// Connects the client to a remote host. Retreives connection settings from internally referenced settings model.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the IP Address is in an invalid format - this method willl throw an invalid operation exception.</exception>
        private void ConnectToRemoteHost()
        {
            if (!IPAddress.TryParse(m_settings.Session.ServerIP, out var ipAddress))
            {
                throw new InvalidOperationException($"Invalid IP Address : Could not parse '{m_settings.Session.ServerIP}' ");
            }

            NotificationService.Instance.Info($"{m_settings.Session.ServerIP}:{m_settings.Session.Port}");
            m_unityTransport.SetConnectionData(m_settings.Session.ServerIP, m_settings.Session.Port);

            if (!m_netManager.StartClient())
            {
                this.LoadSplashScreen();
            };

            m_clientConnecting = true;
        }

        /// <summary>
        /// Looks up and returns the appropriate music/song for the selected level.
        /// </summary>
        /// <param name="level">Game level name</param>
        private void PlayMusicForLevel(string level)
        {
            switch(level)
            {
                case "VolcanicPlanet01":
                    m_audioManager.PlayMusic("Volcanic");
                    break;
                case "Morpheus":
                    m_audioManager.PlayMusic("Ice");
                    break;
                case "SS_Miner01":
                    m_audioManager.PlayMusic("Blackout");
                    break;
                case "Playground":
                    m_audioManager.PlayMusic("Theme");
                    break;
            }
        }

        /// <summary>
        /// Starts a nmew multiplayer session, making the local player/machine host.
        /// </summary>
        /// <exception cref="Exception">Throws an exception if anything other than Started has been returned from LoadScene function call</exception>
        private void StartSessionAsHost()
        {
            m_unityTransport.ConnectionData.Port = m_settings.Session.Port;

            var scene = m_settings.Session.GetSceneName();

            if (m_netManager.StartHost())
            {
                var status = m_netManager.SceneManager.LoadScene(scene, LoadSceneMode.Single);

                switch (status)
                {
                    case SceneEventProgressStatus.None:
                        break;
                    case SceneEventProgressStatus.Started:
                        this.PlayMusicForLevel(m_settings.Session.Level);
                        break;
                    case SceneEventProgressStatus.SceneNotLoaded:
                    case SceneEventProgressStatus.SceneEventInProgress:
                    case SceneEventProgressStatus.InvalidSceneName:
                    case SceneEventProgressStatus.SceneFailedVerification:
                    case SceneEventProgressStatus.InternalNetcodeError:
                    case SceneEventProgressStatus.SceneManagementNotEnabled:
                    case SceneEventProgressStatus.ServerOnlyAction:
                        throw new Exception("Error loading scene : " + status);
                }
            }
        }

        /// <summary>
        /// Laucnhes the game beased upon the settings that have been configured.
        /// -> Change game state to "loading"
        /// -> If player is hosting
        ///     -> Start a new session as host
        ///     -> Initialise the match based upon current settings
        /// -> Else
        ///     -> Connect to remote host base upon current settings   
        ///     -> Lock mouse, and chagne game state to 'in game'
        ///     -> Catch any exceptions, and show them to the player
        /// Reload splash screen</summary>
        public void LaunchGame()
        {
            m_state = GameState.Loading;                                                    

            try
            {
                if (m_settings.Session.IsHost)                                              
                {
                    this.StartSessionAsHost();                                              
                    m_matchManager.InitialiseMatch(m_settings.Session.MatchSettings);       
                }
                else                                                                        
                {
                    this.ConnectToRemoteHost();                                                                     
                }

                this.LockMouse();                                                           
                m_state = GameState.InGame;
            }
            catch (Exception ex)
            {
                NotificationService.Instance.Error(ex);                                     
                MessageBox.Instance.Show(ex);                                               
                this.LoadSplashScreen();
            }
        }

        /// <summary>
        /// Locks the mouse to be in the centre of the screen.
        /// </summary>
        private void LockMouse()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Unlocks mouse such that it can be freely moved around.
        /// </summary>
        private void UnlockMouse()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Loads the splash screen / landing page
        /// </summary>
        public void LoadSplashScreen()
        {
            this.UnlockMouse();
            m_state = GameState.MainMenu;
            SceneManager.LoadScene("SplashScreen", LoadSceneMode.Single);
            m_audioManager.PlayMusic("Theme");
        }

        /// <summary>
        /// Respawns the local player.
        /// </summary>
        public void RespawnLocalPlayer()
        {
            this.NotifyPauseMenuClosed();
            m_spawnManager.RespawnLocalPlayer();
        }

        /// <summary>
        /// Called on any connection event that has been fired.
        /// -> If client has connected
        ///     -> Spawn player / actor for the connectees' client id
        ///     -> Set connecting flag to false
        /// 
        /// -> If client has disconnected
        ///     -> Despawn player
        /// 
        /// -> If client is connecting
        ///     -> If client was diconnected while connecting
        ///     -> if a reson was provided - display reason
        /// -> Else, just say that connection has failed.
        /// </summary>
        /// <param name="arg1">Network manager</param>
        /// <param name="arg2">Connection event data</param>
        private void NetManager_OnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
        {
            if (this.IsServer)
            {
                NotificationService.Instance.Info("Client Id : " + arg2.ClientId + "| Event type : " + arg2.EventType);

                switch (arg2.EventType)
                {
                    case ConnectionEvent.ClientConnected:                          
                        m_spawnManager.SpawnPlayer(arg2.ClientId);                 
                        m_clientConnecting = false;                                
                        break;
                    case ConnectionEvent.ClientDisconnected:                       
                        m_spawnManager.DespawnPlayer(arg2.ClientId);               
                        break;
                }
            }

            if (m_clientConnecting)                                               
            {
                switch (arg2.EventType)
                {
                    case ConnectionEvent.ClientConnected:
                        break;
                    case ConnectionEvent.PeerConnected:
                        break;
                    case ConnectionEvent.ClientDisconnected:                   
                        this.LoadSplashScreen();
                        if (!string.IsNullOrEmpty(arg1.DisconnectReason))      
                        {
                            MessageBox.Instance.Show("Disconnect reason: " + arg1.DisconnectReason);
                        }
                        else
                        {
                            MessageBox.Instance.Show("Connection failed. ");       
                        }

                        break;
                    case ConnectionEvent.PeerDisconnected:
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Unhooks events, and nulls the player entry for the current client Id.
        /// Used for respawning players
        /// </summary>
        /// <param name="clientId">Client id of the player to be deregistered</param>
        public void UnhookPlayerActor(ulong clientId)
        {
            if (m_players.ContainsKey(clientId))
            {
                var playerActor = m_players[clientId];

                var playerState = playerActor.GetComponent<ActorState>();

                playerState.OnStateChanged -= this.PlayerState_OnStateChanged;

                m_players[clientId] = null;

                NotificationService.Instance.Info("Unhooked: Client Id: " + clientId);
            }
        }

        /// <summary>
        /// Deregisters player actor and removes player from the players dictionary.
        /// </summary>
        /// <param name="clientId">Client id of the player to be deregistered</param>
        internal void DeregisterPlayer(ulong clientId)
        {
            if (m_players.ContainsKey(clientId))
            {
                this.UnhookPlayerActor(clientId);
                m_players.Remove(clientId);
                m_matchManager.RemovePlayer(clientId);

                NotificationService.Instance.Info("Deregistered: Client Id: " + clientId);
            }
        }

        /// <summary>
        /// Checks to see if the player is currently registered on this game manager.
        /// </summary>
        /// <param name="clientId">Client Id to be checked</param>
        /// <returns>Exists in players dictionary? (true/false)</returns>
        public bool IsPlayerRegistered(ulong clientId)
        {
            return m_players.ContainsKey(clientId);
        }

        /// <summary>
        /// Registers player with this game manager
        /// </summary>
        /// <param name="clientId">Player client Id</param>
        /// <param name="playerActor">GameObject/Actor associated with this player.</param>
        public void RegisterPlayer(ulong clientId, GameObject playerActor)
        {
            var actorNetwork = playerActor.GetComponent<ActorNetwork>();
            var playerState = playerActor.GetComponent<ActorState>();

            if (actorNetwork.IsLocalPlayer)
            {
                m_localPlayer = playerActor;
            }

            playerState.OnStateChanged += this.PlayerState_OnStateChanged;

            m_players[clientId] = playerActor;

            NotificationService.Instance.Info("Registered : Client Id : " + clientId);

        }


        /// <summary>
        /// Sends off notifications informing any interested parties of a player death.
        /// </summary>
        /// <param name="actor">Player gameobject</param>
        private void HandleActorDeath(GameObject actor)
        {
            NotificationService.Instance.NotifyPlayerKilled(actor);
        }

        /// <summary>
        /// Called whenver the player state within Actor State has been changed.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">State chagned event arguments</param>
        private void PlayerState_OnStateChanged(object sender, Events.OnStateChangedEventArgs e)
        {
            if (e.State.IsDying)
            {
                this.HandleActorDeath(e.Actor);
            }

            if (e.State.IsDead && e.Actor == m_localPlayer)
            {
                m_localPlayerAwaitingRespawn = true;
            }
        }

        /// <summary>
        /// Called on connection approval. This method handles whether a player is allowed to join the game or not.
        ///     -> Refuse connection is player count exceeds 8
        ///     -> Cannot find the prefab for the request
        /// </summary>
        /// <param name="request">Connection approval request</param>
        /// <param name="response">Connection approval response</param>
        private void Netmanager_OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {

            if (m_netManager.ConnectedClients.Count >= 8)                           // Hardcoded player limit of 8 at the moment
            {
                response.Reason = "Max player count reached : 8";
                response.Approved = false;
                return;
            }

            var globalObjHash = BitConverter.ToInt32(request.Payload, 0);          //

            Debug.Log($"Netmanager_OnConnectionApproval ClientId {request.ClientNetworkId} globalObjHash {globalObjHash} ");

            foreach (var prefab in m_netManager.NetworkConfig.Prefabs.Prefabs)
            {
                Debug.Log("Prefab: " + prefab.SourcePrefabGlobalObjectIdHash);

                if (globalObjHash == prefab.SourcePrefabGlobalObjectIdHash)
                {
                    //m_clientPrefabs.Add(request.ClientNetworkId, prefab);
                    response.Approved = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a player actor has been spawned on the scene.
        /// -> Registers the player with this game manager
        /// -> If this machine is server
        ///     -> Check to see wheather the player exists within the match managers' register
        ///     -> Get the match data for the player, either by getting it, or by adding the player to the match registry
        ///     -> Set the actors' team to the team that is stored within the match registry.
        ///     -> Spawn a starter gun for the player, and place it in their inventory.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SpawnManager_OnActorSpawned(object sender, Events.OnActorSpawnedArgs e)
        {
            var netObj = e.Actor.GetComponent<NetworkObject>();
            this.RegisterPlayer(netObj.OwnerClientId, e.Actor);

            var actorState = e.Actor.GetComponent<ActorState>();

            if (this.IsServer)
            {
                var matchData = m_matchManager.GetPlayerMatchData(netObj.OwnerClientId);

                if (matchData == null)
                {
                    matchData = m_matchManager.AddPlayer(netObj.OwnerClientId, e.Actor);
                }

                actorState.Team = matchData.Team;

                var weaponSpawnManager = this.GetWeaponSpawnManager();

                if (weaponSpawnManager != null)
                {
                    var pickup = e.Actor.GetComponent<ActorPickup>();
                    var wpn = weaponSpawnManager.SpawnWeapon(WeaponType.Pistol, Vector3.zero);
                    pickup.PickupWeapon(wpn, false);
                }
            }
        }

        /// <summary>
        /// Called when a player has switched teams
        /// If this machine is server
        ///     -> Move the actor to the correct spawn point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchManager_OnPlayerTeamSwitch(object sender, Events.OnPlayerSwitchTeamsArgs e)
        {
            if (this.IsServer)
            {
                NotificationService.Instance.Info($"Moving client ({e.PlayerData.ClientId}) to spawn.");

                var gameObj = e.PlayerData.Player;

                if (gameObj != null)
                {
                    m_spawnManager.MoveActorToSpawnPoint(e.PlayerData.Player);
                }
            }
        }

        /// <summary>
        /// Called when a client has disconnected from the session
        /// </summary>
        /// <param name="clientId">Id of the client that has been disconnected</param>
        private void NetManager_OnClientDisconnectCallback(ulong clientId)
        {
            this.DeregisterPlayer(clientId);
        }

        /// <summary>
        /// Called when a player actor has despawned
        /// -> Unhooks player actor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpawnManager_OnActorDespawned(object sender, Events.OnActorDespawnedArgs e)
        {
            this.UnhookPlayerActor(e.ClientId);
        }

        /// <summary>
        /// Called when a client has connected to the server
        /// If this machine is server
        ///     -> If the client is not also the host
        ///         -> Send match data to the client 
        /// </summary>
        /// <param name="clientId">Client Id</param>
        private void Netmanager_OnClientConnectedCallback(ulong clientId)
        {
            if (this.IsServer)
            {
                NotificationService.Instance.Info("ClientId: " + clientId);

                if (clientId != NetworkManager.ServerClientId)
                {
                    var matchManNet = m_matchManager.GetComponent<MatchManagerNetwork>();
                    matchManNet.SendMatchDataToClient(clientId);

                    // this.SpawnPlayer(clientId);
                }
            }

        }

        /// <summary>
        /// Called when match state has changed on match manager
        ///     -> Depending on match state
        ///         -> Initialise match
        ///         -> Respawn all clients
        /// -> Despawn all registered weapons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchManager_OnMatchStateChanged(object sender, EventArgs e)
        {
            var data = m_matchManager.GetMatchData();

            switch (data.MatchState)
            {
                case MatchState.PendingStart:
                    m_matchManager.InitialiseMatch(m_settings.Session.MatchSettings, MatchState.PendingStart);
                    m_spawnManager.RespawnAllClients();

                    break;
                case MatchState.Running:
                    m_matchManager.InitialiseMatch(m_settings.Session.MatchSettings, MatchState.Running);
                    m_spawnManager.RespawnAllClients();
                    break;
                case MatchState.Ended:
                    break;
            }

            var wpnSpawnManager = this.GetWeaponSpawnManager();

            if (wpnSpawnManager != null)
            {
                wpnSpawnManager.DespawnAllRegisteredWeapons();
            }

        }
    }
}