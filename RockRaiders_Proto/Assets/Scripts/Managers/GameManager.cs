﻿using Assets.Scripts.Input;
using Assets.Scripts.UI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
    public struct SetStateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GameManager : MonoBehaviour
    {
        public event EventHandler<EventArgs> OnRespawnTriggered;

        private bool m_playerPaused;

        public bool PlayerPaused
        {
            get
            {
                return m_playerPaused;
            }
        }


        [SerializeField]
        private NetworkManager m_netManager;

        [SerializeField]
        private InputManager m_inputManager;

        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                return _instance = _instance ?? new GameManager();
            }
        }

        private GameState m_state;
        private SettingsModel m_settings;
        
        private UnityTransport m_unityTransport;

        private Dictionary<ulong, NetworkPrefab> m_clientPrefabs;

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
            if (!IPAddress.TryParse(m_settings.MatchSettings.ServerIP, out var ipAddress))
            {
                throw new InvalidOperationException($"Invalid IP Address : Could not parse '{m_settings.MatchSettings.ServerIP}' ");
            }

            m_unityTransport.SetConnectionData(m_settings.MatchSettings.ServerIP, m_settings.MatchSettings.Port);
            m_netManager.StartClient();
        }

        private void StartSessionAsHost()
        {
            m_unityTransport.ConnectionData.Port = m_settings.MatchSettings.Port;

            var scene = m_settings.MatchSettings.GetSceneName();

            //m_netManager.ConnectionApprovalCallback += this.Netmanager_OnConnectionApproval;
            //m_netManager.OnClientConnectedCallback += this.Netmanager_OnClientConnectedCallback;
            
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

        public void LaunchGame()
        {
            m_state = GameState.Loading;

            try
            {
                if (m_settings.MatchSettings.IsHost)
                {
                    this.StartSessionAsHost();
                }
                else
                {
                    this.ConnectToRemoteHost();
                }

                this.LockMouse();
            }
            catch(Exception ex)
            {
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
            SceneManager.LoadScene("SplashScreen", LoadSceneMode.Single);
            m_state = GameState.MainMenu;
        }

        internal void RespawnPlayer()
        {
            this.NotifyPauseMenuClosed();
            this.OnRespawnTriggered?.Invoke(this, EventArgs.Empty);
        }


        //private void Netmanager_OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        //{
        //    var globalObjHash = BitConverter.ToInt32(request.Payload, 0);

        //    Debug.Log($"Netmanager_OnConnectionApproval ClientId {request.ClientNetworkId} globalObjHash {globalObjHash} ");

        //    foreach(var prefab in m_netManager.NetworkConfig.Prefabs.Prefabs)
        //    {
        //        Debug.Log("Prefab: " + prefab.SourcePrefabGlobalObjectIdHash);

        //        if (globalObjHash == prefab.SourcePrefabGlobalObjectIdHash)
        //        {
        //            m_clientPrefabs.Add(request.ClientNetworkId, prefab);
        //            response.Approved = true;
        //            break;
        //        }
        //    }
        //}

        //private void SpawnPlayer(ulong clientId)
        //{
        //    if (m_clientPrefabs.TryGetValue(clientId, out var clientPrefab))
        //    {
        //        var playerObj = GameObject.Instantiate(clientPrefab.Prefab);
        //        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        //    }
        //}

        //private void Netmanager_OnClientConnectedCallback(ulong clientId)
        //{
        //    Debug.Log("Netmanager_OnClientConnectedCallback clientId : " + clientId);

        //    if (clientId != NetworkManager.ServerClientId)
        //    {
        //        this.SpawnPlayer(clientId);
        //    }
        //}

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