using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Input;
using Assets.Scripts.Network;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Managers
{
    [Serializable]
    public class SceneSpawnSettings
    {
        

        [SerializeField]
        private UIGameOverlay m_uiOverlay;

        public UIGameOverlay UIGameOverlay
        {
            get
            {
                return m_uiOverlay;
            }
        }


        public SceneSpawnSettings()
        {
        }
    }

    public class ActorSpawnManager : NetworkBehaviour
    {
        private bool m_isReady;
        private static ActorSpawnManager _instance;
        private List<SpawnPoint> m_spawnPoints;

        public static ActorSpawnManager Instance
        {
            get
            {
                return _instance = _instance ?? new ActorSpawnManager();
            }
        }

        public GameObject PlayerPrefab
        {
            get
            {
                return m_playerPrefab;
            }
        }

        public event EventHandler<OnActorSpawnedArgs> OnActorSpawn;
        public event EventHandler<OnActorDespawnedArgs> OnActorDespawn;


        [SerializeField]
        private GameObject m_cameraManager;

        [SerializeField]
        private GameObject m_playerPrefab;

        [SerializeField]
        private GameObject m_cameraPrefab;

        private Dictionary<ulong, NetworkObject> m_clients;

        private HashSet<ulong> m_pendingSpawn;
        private HashSet<ulong> m_pendingDespawn;

        //[SerializeField]
        //private GameManager m_gameManager;

        [Header("Scene Settings (Leave blank in splash screen)", order = 2)]
        [SerializeField]
        private SceneSpawnSettings m_sceneSettings;

        // private static Dictionary<ulong, string> playerNameDictionary;

        public ActorSpawnManager()
        {
            m_spawnPoints = new List<SpawnPoint>();
            m_sceneSettings = new SceneSpawnSettings();
            //m_clients = new Dictionary<ulong, NetworkObject>();
            m_pendingSpawn = new HashSet<ulong>();
            m_pendingDespawn = new HashSet<ulong>();
            m_clients = new Dictionary<ulong, NetworkObject>();
            //playerNameDictionary = new Dictionary<ulong, string>();
        }

        private void Start()
        {

        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Eh.... Will think of a better way to handle this.
                _instance.m_sceneSettings = m_sceneSettings;
                _instance.Initialise();
                Destroy(base.gameObject);
            }

            
        }

        public void Initialise()
        {
            m_spawnPoints.Clear();
            m_spawnPoints = this.GetAllSpawnPointsForScene(SceneManager.GetActiveScene());
        }

        private List<SpawnPoint> GetAllSpawnPointsForScene(Scene scene)
        {
            var result = new List<SpawnPoint>();
            var rootObjs = scene.GetRootGameObjects();

            foreach (var obj in rootObjs)
            {
                var spawnPoints = obj.GetComponentsInChildren<SpawnPoint>();
                result.AddRange(spawnPoints);
            }

            return result;
        }

        private void Update()
        {
            _instance.m_isReady = SceneManager.GetActiveScene().name != "SplashScreen" && SceneManager.GetActiveScene().isLoaded;

            if (m_isReady)
            {
                foreach (var clientId in m_pendingDespawn)
                {
                    this.DespawnClient(clientId);
                }

                foreach (var clientId in m_pendingSpawn)
                {
                    this.SpawnClient(clientId);
                    //this.SpawnPlayer(clientId);
                }

                m_pendingDespawn.Clear();
                m_pendingSpawn.Clear();
            }
        }

        public void SpawnPlayer(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
        }

        public void DespawnPlayer(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
        }

        public void SpawnLocalPlayer()
        {
            //m_network.SpawnClientServerRpc();
            //this.OnServerSpawnPlayerCalled?.Invoke(this, EventArgs.Empty);
            //SpawnPlayerServerRpc();
            this.SpawnClientServerRpc();
        }

        public void DespawnLocalPlayer()
        {
            //m_network.DespawnClientServerRpc();
            //this.OnServerDespawnPlayerCalled?.Invoke(this, EventArgs.Empty);
            //DespawnPlayerServerRpc();

            this.DespawnClientServerRpc();
        }

        public void RespawnLocalPlayer()
        {
            DespawnLocalPlayer();
            SpawnLocalPlayer();
        }

        private void InitialiseBehaviours(GameObject actor)
        {
            var behaviours = actor.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                behaviour.Initialise();
            }
        }

        public void SpawnClient(ulong clientId)
        {
            NotificationService.Instance.Info($"ClientId: {clientId}");

            // Instantiate player object
            GameObject player = GameObject.Instantiate(m_playerPrefab);

            this.InitialiseBehaviours(player);

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = this;

            //var spawnPoint = this.GetSpawnPoint(player);
            //player.transform.position = spawnPoint.transform.position;

            var playerNet = player.GetComponent<NetworkObject>();

            playerNet.SpawnAsPlayerObject(clientId, true);

            m_clients[clientId] = playerNet;
        }

        public void DespawnClient(ulong clientId)
        {
            NotificationService.Instance.Info($"ClientId: {clientId}");

            if (m_clients.ContainsKey(clientId))
            {
                var playerNetworkObject = m_clients[clientId];
                bool canDespawn;

                if (this.NetworkManager == null || this.NetworkManager.SpawnManager == null)
                {
                    canDespawn = false;
                }
                else
                {
                    var spawnedObjects = this.NetworkManager.SpawnManager.SpawnedObjects;
                    canDespawn = spawnedObjects.ContainsKey(playerNetworkObject.NetworkObjectId);
                }


                if (playerNetworkObject != null && canDespawn)
                {
                    playerNetworkObject.Despawn(true);
                }

                m_clients.Remove(clientId);
            }
        }

        public void RespawnAllClients()
        {
            if (this.IsServer)
            {
                var clientIds = m_clients.Keys.ToArray();

                foreach (var clientId in clientIds)
                {
                    this.DespawnClient(clientId);
                    this.SpawnClient(clientId);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnClientServerRpc(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
            //m_spawnManager.SpawnPlayer(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnClientServerRpc(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
            //m_spawnManager.DespawnPlayer(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingSpawn.Add(senderClientId);
            //m_spawnManager.SpawnPlayer(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingDespawn.Add(senderClientId);
            //m_spawnManager.DespawnPlayer(senderClientId);
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
            if (m_cameraManager == null)
            {
                throw new NullReferenceException("No 'CameraSystem' has been set.");
            }

            var cameraSystem = m_cameraManager.GetComponent<CameraManager>();

            var cameraObj = GameObject.Instantiate(m_cameraPrefab);
            var actorCamera = cameraObj.GetComponent<ActorCamera>();
            actorCamera.Target = actor;
            //actorCamera.Offset = new Vector2(0.5f, 0.5f);
            //actorCamera.Distance = 1.0f;

            cameraObj.AddComponent<Camera>();

            var camera = cameraObj.GetComponent<Camera>();

            actor.GetComponent<ActorFloating>().ActorCamera = actorCamera;
            actor.GetComponent<ActorGrounded>().ActorCamera = actorCamera;
            actor.GetComponent<ActorCrosshair>().ActorCamera = actorCamera;

            cameraSystem.AddCamera(camera, isLocal);
        }

        public void SetupUIOverlay(GameObject actor)
        {
            m_sceneSettings.UIGameOverlay.Actor = actor.GetComponent<ActorController>();
        }

        public GameObject GetSpawnPoint(GameObject actor)
        {
            var state = actor.GetComponent<ActorState>();
            var spawmPoints = m_spawnPoints.Where(x => x.Team == state.Team).ToList();
            var rndIdx = Random.Range(0, spawmPoints.Count - 1);

            if (spawmPoints.Count > rndIdx && spawmPoints[rndIdx] != null)
            {
                return spawmPoints[rndIdx].gameObject;
            }

            return null;
        }

        public void PrepareLocalPlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.RegisterActorOnInputManager(actor);
            this.CreateActorCamera(actor, true);
            this.SetupUIOverlay(actor);

            this.MoveActorToSpawnPoint(actor);
            this.OnActorSpawn?.Invoke(this, new OnActorSpawnedArgs(actor));
        }

        public void PrepareRemotePlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.CreateActorCamera(actor, false);
            this.MoveActorToSpawnPoint(actor);
            this.OnActorSpawn?.Invoke(this, new OnActorSpawnedArgs(actor));
        }

        public void MoveActorToSpawnPoint(GameObject actor)
        {
            var spawnPoint = this.GetSpawnPoint(actor);

            if (spawnPoint != null)
            {
                actor.transform.position = spawnPoint.transform.position;
            }
        }
    }
}
