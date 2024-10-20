using Assets.Scripts.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.Managers
{
    public struct SetStateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GameManager
    {
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
        private NetworkManager m_netManager;
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
            m_netManager = NetworkManager.Singleton;
            m_unityTransport = m_netManager.GetComponent<UnityTransport>();
            m_state = GameState.InGame;
            m_settings = new SettingsModel();

            m_clientPrefabs = new Dictionary<ulong, NetworkPrefab>();
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
            }
            catch(Exception ex)
            {
                this.LoadSplashScreen();
            }
        }

        public void LoadSplashScreen()
        {
            SceneManager.LoadScene("UI_SplashScreen", LoadSceneMode.Single);
            m_state = GameState.MainMenu;
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