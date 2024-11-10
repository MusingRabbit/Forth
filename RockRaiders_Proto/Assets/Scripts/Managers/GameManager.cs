using Assets.Scripts.Actor;
using Assets.Scripts.Input;
using Assets.Scripts.Managers;
using Assets.Scripts.Network;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Network
{
    public struct SetStateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GameManager : NetworkBehaviour, IGameManager
    {
        private bool m_playerPaused;
        private bool m_localPlayerAwaitingRespawn;
        private bool m_clientConnecting;

        private Dictionary<ulong, GameObject> m_players;
        private GameObject m_localPlayer;

        public bool PlayerPaused
        {
            get
            {
                return m_playerPaused;
            }
        }

        public bool LocalPlayerAwaitingRespawn
        {
            get
            {
                return m_localPlayerAwaitingRespawn;
            }
        }


        [SerializeField]
        private NetworkManager m_netManager;

        [SerializeField]
        private InputManager m_inputManager;

        [SerializeField]
        private ActorSpawnManager m_spawnManager;

        [SerializeField]
        private MatchManager m_matchManager;

        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private GameState m_state;
        private SettingsModel m_settings;

        private UnityTransport m_unityTransport;

        private Dictionary<ulong, NetworkPrefab> m_clientPrefabs;

        public bool InGame
        {
            get
            {
                return m_state == GameState.InGame;
            }
        }

        public SettingsModel Settings
        {
            get
            {
                return m_settings;
            }
        }

        public GameManager()
        {
            m_state = GameState.MainMenu;
            m_settings = new SettingsModel();

            m_clientPrefabs = new Dictionary<ulong, NetworkPrefab>();
            m_players = new Dictionary<ulong, GameObject>();
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                GameObject.DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                GameObject.Destroy(base.gameObject);
            }
        }

        private void Start()
        {
            m_unityTransport = m_netManager.GetComponent<UnityTransport>();
            m_netManager.OnConnectionEvent += this.NetManager_OnConnectionEvent;

            m_spawnManager.OnActorSpawn += this.SpawnManager_OnActorSpawned;
            m_spawnManager.OnActorDespawn += this.SpawnManager_OnActorDespawned;

            m_matchManager.Initialise(this);

            m_matchManager.OnMatchStateChanged += MatchManager_OnMatchStateChanged;
            m_matchManager.OnPlayerTeamSwitch += MatchManager_OnPlayerTeamSwitch;

            m_netManager.ConnectionApprovalCallback += this.Netmanager_OnConnectionApproval;
            m_netManager.OnClientConnectedCallback += this.Netmanager_OnClientConnectedCallback;
            m_netManager.OnClientDisconnectCallback += this.NetManager_OnClientDisconnectCallback;
        }

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

            if (m_matchManager.IsReady)
            {
                //this.AddAllPlayersToMatch();
            }
        }

        public void QuitGame()
        {
            this.NotifyPauseMenuClosed();
            m_netManager.Shutdown();
            this.LoadSplashScreen();
        }

        public void NotifyPauseMenuClosed()
        {
            m_inputManager.enabled = true;
            m_playerPaused = false;
            this.LockMouse();
            m_inputManager.Controller.SetActionState(ControllerActions.Pause, ActionState.InActive);
        }

        private void ConnectToRemoteHost()
        {
            if (!IPAddress.TryParse(m_settings.Session.ServerIP, out var ipAddress))
            {
                throw new InvalidOperationException($"Invalid IP Address : Could not parse '{m_settings.Session.ServerIP}' ");
            }

            NotificationService.Instance.Info($"{m_settings.Session.ServerIP}:{m_settings.Session.Port}");
            m_unityTransport.SetConnectionData(m_settings.Session.ServerIP, m_settings.Session.Port);

            //m_netManager.ConnectionApprovalCallback += this.Netmanager_ConnectionCheck;

            if (!m_netManager.StartClient())
            {
                this.LoadSplashScreen();
            };

            m_clientConnecting = true;
        }

        private void StartSessionAsHost()
        {
            m_unityTransport.ConnectionData.Port = m_settings.Session.Port;

            var scene = m_settings.Session.GetSceneName();

            if (m_netManager.StartHost())
            {
                //m_netManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
                var status = m_netManager.SceneManager.LoadScene(scene, LoadSceneMode.Single);

                switch (status)
                {
                    case SceneEventProgressStatus.None:
                        break;
                    case SceneEventProgressStatus.Started:
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

        private void NetManager_OnClientDisconnectCallback(ulong clientId)
        {
            this.DeregisterPlayer(clientId);
        }

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

        private void LockMouse()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockMouse()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LoadSplashScreen()
        {
            this.UnlockMouse();
            m_state = GameState.MainMenu;
            SceneManager.LoadScene("SplashScreen", LoadSceneMode.Single);

        }

        public void RespawnPlayer()
        {
            this.NotifyPauseMenuClosed();
            m_spawnManager.RespawnLocalPlayer();
        }



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

                        if (m_clientConnecting)
                        {
                            MessageBox.Instance.Show("Connection failed. ");
                        }

                        if (!string.IsNullOrEmpty(arg1.DisconnectReason))
                        {
                            MessageBox.Instance.Show("Disconnect reason: " + arg1.DisconnectReason);
                        }

                        break;
                    case ConnectionEvent.PeerDisconnected:
                        break;
                    default:
                        break;
                }
            }
        }

        public void DetatchPlayerActor(ulong clientId)
        {
            if (m_players.ContainsKey(clientId))
            {
                var playerActor = m_players[clientId];

                var actorNetwork = playerActor.GetComponent<ActorNetwork>();
                var playerState = playerActor.GetComponent<ActorState>();

                playerState.OnStateChanged -= this.PlayerState_OnStateChanged;

                m_players[clientId] = null;

                NotificationService.Instance.Info("Deregistered: Client Id: " + clientId);
            }
        }

        internal void DeregisterPlayer(ulong clientId)
        {
            if (m_players.ContainsKey(clientId))
            {
                this.DetatchPlayerActor(clientId);
                m_players.Remove(clientId);
                m_matchManager.RemovePlayer(clientId);

                NotificationService.Instance.Info("Deregistered: Client Id: " + clientId);
            }
        }

        public bool IsPlayerRegistered(ulong clientId)
        {
            return m_players.ContainsKey(clientId);
        }

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

        private void AddAllPlayersToMatch()
        {
            foreach (var kvp in m_players)
            {
                var player = kvp.Value;
                var clientId = kvp.Key;

                if (player != null)
                {
                    if (!m_matchManager.PlayerExists(clientId))
                    {
                        m_matchManager.AddPlayer(clientId, player);
                    }
                }
            }
        }


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

        private void HandleActorDeath(GameObject actor)
        {
            NotificationService.Instance.NotifyPlayerKilled(actor);
        }

        private void Netmanager_ConnectionCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = false;
            response.Pending = true;

            this.StartCoroutine(AwaitTimeout(response));
        }

        IEnumerator AwaitTimeout(NetworkManager.ConnectionApprovalResponse response)
        {
            yield return new WaitForSeconds(5.0f);
            response.Approved = true;
            response.Pending = false;
        }

        private void Netmanager_OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {

            if (m_netManager.ConnectedClients.Count >= 8)
            {
                response.Reason = "Max player count reached : 8";
                response.Approved = false;
                return;
            }

            var globalObjHash = BitConverter.ToInt32(request.Payload, 0);

            Debug.Log($"Netmanager_OnConnectionApproval ClientId {request.ClientNetworkId} globalObjHash {globalObjHash} ");

            foreach (var prefab in m_netManager.NetworkConfig.Prefabs.Prefabs)
            {
                Debug.Log("Prefab: " + prefab.SourcePrefabGlobalObjectIdHash);

                if (globalObjHash == prefab.SourcePrefabGlobalObjectIdHash)
                {
                    m_clientPrefabs.Add(request.ClientNetworkId, prefab);
                    response.Approved = true;
                    break;
                }
            }
        }

        private void SpawnManager_OnActorSpawned(object sender, Events.OnActorSpawnedArgs e)
        {
            var netObj = e.Actor.GetComponent<NetworkObject>();
            this.RegisterPlayer(netObj.OwnerClientId, e.Actor);

            if (this.IsServer)
            {
                var matchData = m_matchManager.GetPlayerMatchData(netObj.OwnerClientId);

                if (matchData != null)
                {
                    var actorState = e.Actor.GetComponent<ActorState>();
                    actorState.Team = matchData.Team;
                }
                else
                {
                    matchData = m_matchManager.AddPlayer(netObj.OwnerClientId, e.Actor);

                    var actorState = e.Actor.GetComponent<ActorState>();
                    actorState.Team = matchData.Team;
                }
            }
        }

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


        private void SpawnManager_OnActorDespawned(object sender, Events.OnActorDespawnedArgs e)
        {
            this.DetatchPlayerActor(e.ClientId);
        }

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


        }

        //private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
        //{
        //    Debug.Log($"SceneManager_OnSceneEvent {sceneEvent.SceneName} clientId : {sceneEvent.ClientId} event {sceneEvent.SceneEventType}");

        //    if (m_netManager.IsHost)
        //    {
        //        switch (sceneEvent.SceneEventType)
        //        {
        //            case SceneEventType.LoadEventCompleted:
        //                this.SpawnPlayer(sceneEvent.ClientId);
        //                break;
        //        }
        //    }
        //}
    }
}