using Assets.Scripts.Actor;
using Assets.Scripts.Input;
using Assets.Scripts.Managers;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Factory
{
    public class ActorSpawnManager : NetworkBehaviour
    {
        private static ActorSpawnManager instance;

        public static ActorSpawnManager Instance
        {
            get
            {
                return instance = instance ?? new ActorSpawnManager();
            }
        }


        [SerializeField]
        private GameObject m_cameraManagerObj;

        [SerializeField]
        private GameObject m_playerPrefab;

        [SerializeField]
        private GameManager m_gameManager;

        [SerializeField]
        private List<SpawnPoint> m_spawnPoints;

        [SerializeField]
        private UIGameOverlay m_uiOverlay;


        private Dictionary<ulong, NetworkObject> m_clients;
       // private static Dictionary<ulong, string> playerNameDictionary;
        
        public ActorSpawnManager()
        {
            instance = this;
            m_spawnPoints = new List<SpawnPoint>();
            m_clients = new Dictionary<ulong, NetworkObject>();
            //playerNameDictionary = new Dictionary<ulong, string>();
        }

        public void Awake()
        {
            if (m_gameManager == null)
            {
                m_gameManager = GameManager.Instance;
                m_gameManager.OnRespawnTriggered += GameManager_OnRespawnTriggered;
            }
        }

        public void Start()
        {
            if (m_gameManager == null)
            {
                m_gameManager = GameManager.Instance;
                m_gameManager.OnRespawnTriggered += GameManager_OnRespawnTriggered;
            }
        }

        public void Update()
        {
        }

        public override void OnNetworkSpawn()
        {
            this.SpawnPlayerServerRpc();
        }

        public void SpawnPlayer()
        {
            this.SpawnPlayerServerRpc();
        }

        public void DespawnPlayer()
        {
            this.DespawnPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            this.SpawnPlayerOnClients(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            this.DespawnPlayerOnClients(senderClientId);
        }

        //[Rpc(SendTo.Server)]
        //private void SetPlayerNameServerRpc(ulong clientId, string playerName)
        //{
        //    if (playerNameDictionary.ContainsKey(clientId))
        //    {
        //        playerNameDictionary[clientId] = playerName;
        //    }

        //    else
        //    {
        //        playerNameDictionary.Add(clientId, playerName);
        //    }
            
        //    this.SendUpdatedPlayerNamesClientRpc(JsonConvert.SerializeObject(playerNameDictionary));
        //}

        //[Rpc(SendTo.ClientsAndHost)]
        //private void SendUpdatedPlayerNamesClientRpc(string dictJson)
        //{
        //    var dict = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(dictJson);
        //    playerNameDictionary = dict;
        //}


        private void SpawnPlayerOnClients(ulong clientId)
        {
            NotificationService.Instance.Info($"ClientId: {clientId}");

            // Instantiate player object
            GameObject player = GameObject.Instantiate(m_playerPrefab);

            this.InitialiseBehaviours(player);

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = this;

            var spawnPoint = this.GetSpawnPoint(player);
            player.transform.position = spawnPoint.transform.position;

            // Get the NetworkObject component
            var playerNetworkObject = player.GetComponent<NetworkObject>();

            // Spawn the player object on all clients
            playerNetworkObject.SpawnAsPlayerObject(clientId);

            m_clients[clientId] = playerNetworkObject;
            
        }

        private void DespawnPlayerOnClients(ulong clientId)
        {
            NotificationService.Instance.Info($"ClientId: {clientId}");

            var playerNetworkObject = m_clients[clientId];

            m_gameManager.DeregisterPlayer(clientId);

            playerNetworkObject.Despawn(true);
        }

        private void InitialiseBehaviours(GameObject actor)
        {
            var behaviours = actor.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                behaviour.Initialise();
            }
        }

        public void SetupActorNetworkComponent(GameObject actor)
        {
            if (actor == null)
            {
                throw new NullReferenceException(nameof(actor));
            }

            var cmpActorNetwork = actor.GetComponent<ActorNetwork>();
            cmpActorNetwork.ActorSpawnManager = this;
        }

        public void RegisterActorOnInputManager(GameObject actor)
        {
            if (actor == null)
            {
                throw new NullReferenceException(nameof(actor));
            }

            var controller = actor.GetComponent<PlayerInput>();
            InputManager.Instance.RegisterPlayerController(controller);
        }

        public void CreateActorCamera(GameObject actor, bool isLocal)
        {
            if (m_cameraManagerObj == null)
            {
                throw new NullReferenceException("No 'CameraSystem' has been set.");
            }

            var cameraSystem = m_cameraManagerObj.GetComponent<CameraManager>();
            var cameraObj = new GameObject();
            cameraObj.name = "Actor Camera";
            cameraObj.AddComponent<ActorCamera>();

            var actorCamera = cameraObj.GetComponent<ActorCamera>();
            actorCamera.Target = actor;
            actorCamera.Offset = new Vector2(0.5f, 0.5f);
            actorCamera.Distance = 1.0f;
            
            cameraObj.AddComponent<Camera>();

            var camera = cameraObj.GetComponent<Camera>();

            actor.GetComponent<ActorFloating>().ActorCamera = actorCamera;
            actor.GetComponent<ActorGrounded>().ActorCamera = actorCamera;
            actor.GetComponent<ActorCrosshair>().ActorCamera = actorCamera;

            cameraSystem.AddCamera(camera, isLocal);
        }

        public void SetupUIOverlay(GameObject actor)
        {
            m_uiOverlay.Actor = actor.GetComponent<ActorController>();
        }

        private GameObject GetSpawnPoint(GameObject actor)
        {
            var controller = actor.GetComponent<ActorController>();
            var spawmPoints = m_spawnPoints.Where(x => x.Team == controller.Team).ToList();
            var rndIdx = Random.Range(0, spawmPoints.Count - 1);
            return spawmPoints[rndIdx].gameObject;
        }

        public void PrepareLocalPlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.RegisterActorOnInputManager(actor);
            this.CreateActorCamera(actor, true);
            this.SetupUIOverlay(actor);

            var netObj = actor.GetComponent<ActorNetwork>();

            //this.SetupPlayerName(netObj);

            m_gameManager.RegisterPlayer(netObj.OwnerClientId, actor);
        }

        //private void SetupPlayerName(ActorNetwork netObj)
        //{
        //    var playerName = m_gameManager.Settings.GameSettings.PlayerName;

        //    if (this.IsServer)
        //    {
        //        playerNameDictionary[netObj.OwnerClientId] = playerName;
        //    }
        //    else
        //    {
        //        this.SetPlayerNameServerRpc(netObj.OwnerClientId, playerName);
        //    }

        //    netObj.PlayerName = playerName;
        //}

        public void PrepareRemotePlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.CreateActorCamera(actor, false);

            var netObj = actor.GetComponent<ActorNetwork>();

            //if (playerNameDictionary.ContainsKey(netObj.OwnerClientId))
            //{
            //    netObj.PlayerName = playerNameDictionary[netObj.OwnerClientId];
            //}

            m_gameManager.RegisterPlayer(netObj.OwnerClientId, actor);
        }

        private void GameManager_OnRespawnTriggered(object sender, EventArgs e)
        {
            this.DespawnPlayer();
            this.SpawnPlayer();
        }
    }
}
