using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Input;
using Assets.Scripts.Network;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
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

    /// <summary>
    /// Manages the spawning of player actors
    /// </summary>
    public class ActorSpawnManager : NetworkBehaviour
    {
        /// <summary>
        /// Singleton static instance
        /// </summary>
        private static ActorSpawnManager _instance;

        /// <summary>
        /// Gets singleton static instance
        /// </summary>
        public static ActorSpawnManager Instance
        {
            get
            {
                return _instance = _instance ?? new ActorSpawnManager();
            }
        }

        /// <summary>
        /// Flag to denote wheather the actor spawner can be used.
        /// </summary>
        private bool m_isReady;

        /// <summary>
        /// Stores a list of all available spawn points.
        /// </summary>
        private List<SpawnPoint> m_spawnPoints;

        /// <summary>
        /// Stores a reference to the camera manager game object
        /// </summary>
        [SerializeField]
        private GameObject m_cameraManager;

        /// <summary>
        /// Stores a reference to the player prefab gameobject
        /// </summary>
        [SerializeField]
        private GameObject m_playerPrefab;

        /// <summary>
        /// Stores a reference to the camera prefab gameobject
        /// </summary>
        [SerializeField]
        private GameObject m_cameraPrefab;

        /// <summary>
        /// Stores a list of all networked clients
        /// </summary>
        private Dictionary<ulong, NetworkObject> m_clients;

        /// <summary>
        /// Stores a set of clientId's pending spawn
        /// </summary>
        private HashSet<ulong> m_pendingSpawn;

        /// <summary>
        /// Stores a set of clientId's pending despawn
        /// </summary>
        private HashSet<ulong> m_pendingDespawn;

        /// <summary>
        /// Gets the player prefab
        /// </summary>
        public GameObject PlayerPrefab
        {
            get
            {
                return m_playerPrefab;
            }
        }

        /// <summary>
        /// Invoked whenever a player actor has been spawned
        /// </summary>
        public event EventHandler<OnActorSpawnedArgs> OnActorSpawn;

        /// <summary>
        /// Invoked whenever a player actor has been despawned
        /// </summary>
        public event EventHandler<OnActorDespawnedArgs> OnActorDespawn;

        /// <summary>
        /// Scene spawn settings.
        /// </summary>
        [Header("Scene Settings (Leave blank in splash screen)", order = 2)]
        [SerializeField]
        private SceneSpawnSettings m_sceneSettings;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorSpawnManager()
        {
            m_spawnPoints = new List<SpawnPoint>();
            m_sceneSettings = new SceneSpawnSettings();
            m_pendingSpawn = new HashSet<ulong>();
            m_pendingDespawn = new HashSet<ulong>();
            m_clients = new Dictionary<ulong, NetworkObject>();
        }

        /// <summary>
        /// Called before first frame
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Called on load
        ///     -> Sets singleton instance and 'do not destroy on load' flags
        /// </summary>
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

        /// <summary>
        /// Called every frame.
        ///-> Checks if ready
        ///-> If ready 
        ///    -> Process all clients pending spawn and despawn.
        /// </summary>
        private void Update()
        {
            var activeScene = SceneManager.GetActiveScene();
            m_isReady = activeScene.name != "SplashScreen" && activeScene.isLoaded;

            if (m_isReady)
            {
                foreach (var clientId in m_pendingDespawn)
                {
                    this.DespawnClient(clientId);
                }

                foreach (var clientId in m_pendingSpawn)
                {
                    this.SpawnClient(clientId);
                }

                m_pendingDespawn.Clear();
                m_pendingSpawn.Clear();
            }
        }


        /// <summary>
        /// Initialisation
        ///     -> Refreshes spawn points for all spawn points in active scene
        /// </summary>
        public void Initialise()
        {
            m_spawnPoints.Clear();
            m_spawnPoints = this.GetAllSpawnPointsForScene(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Gets all spawn points for a given scene.
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <returns>A list of spawn points that are contained within that scene.</returns>
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

        /// <summary>
        /// Add client id to pending spawn set
        /// </summary>
        /// <param name="clientId">Client Id</param>
        public void SpawnPlayer(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
        }

        /// <summary>
        /// Adds client id to pending despawn set
        /// </summary>
        /// <param name="clientId">client id</param>
        public void DespawnPlayer(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
        }

        /// <summary>
        /// Makes a request to the server to spawn the local player
        /// </summary>
        public void SpawnLocalPlayer()
        {
            this.SpawnClientServerRpc();
        }

        /// <summary>
        /// Makes a request to the server to despawn the local player
        /// </summary>
        public void DespawnLocalPlayer()
        {
            this.DespawnClientServerRpc();
        }

        /// <summary>
        /// Makes server requests to despawn and then spawn the local player
        /// </summary>
        public void RespawnLocalPlayer()
        {
            DespawnLocalPlayer();
            SpawnLocalPlayer();
        }

        /// <summary>
        /// Calls initialise on instantiated actor objects where .Start() has not been called.
        /// </summary>
        /// <param name="actor">Actor to initialise</param>
        private void InitialiseBehaviours(GameObject actor)
        {
            var behaviours = actor.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                behaviour.Initialise();
            }
        }

        /// <summary>
        /// Spawns client for client Id
        /// -> Instantiates player object
        /// -> Initialises player object
        /// -> Spawns player object on the network
        /// -> Add player to m_clients dictionary
        /// </summary>
        /// <param name="clientId"></param>
        public void SpawnClient(ulong clientId)
        {
            NotificationService.Instance.Info($"ClientId: {clientId}");

            // Instantiate player object
            GameObject player = GameObject.Instantiate(m_playerPrefab);

            this.InitialiseBehaviours(player);

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = this;

            var playerNet = player.GetComponent<NetworkObject>();

            playerNet.SpawnAsPlayerObject(clientId, true);

            m_clients[clientId] = playerNet;
        }

        /// <summary>
        /// Despawns client for client Id
        /// -> Checks to see if client can be despawned from the network
        ///     -> If has access to network manager, clientId is registered with this actor spawn manager, and if the spawnedObjects of the netowork manager contains the network object id for the client player object.
        ///         -> Despawn client player object
        ///     -> Remove client from clients dictionary
        /// </summary>
        /// <param name="clientId"></param>
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

                this.OnActorDespawn?.Invoke(this, new OnActorDespawnedArgs(clientId));
            }
        }

        /// <summary>
        /// Respawns all clients within the clients dictionary.
        /// </summary>
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

        /// <summary>
        /// Sends a request to server to spawn the client
        /// Adds request to servers' pendingspawn set
        /// </summary>
        /// <param name="clientId">client id</param>
        [ServerRpc(RequireOwnership = false)]
        private void SpawnClientServerRpc(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
        }

        /// <summary>
        /// Sends a request to server to despawn the client
        /// Adds request to servers' pending despawn set
        /// </summary>
        /// <param name="clientId">client id</param>
        [ServerRpc(RequireOwnership = false)]
        private void DespawnClientServerRpc(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
        }

        /// <summary>
        /// Sends a request to server to spawn the client
        /// Adds request to servers' pendingspawn set
        /// </summary>
        /// <param name="rpcParams">Server RPC Params</param>
        [ServerRpc(RequireOwnership = false)]
        private void SpawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingSpawn.Add(senderClientId);
            //m_spawnManager.SpawnPlayer(senderClientId);
        }


        /// <summary>
        /// Sends a request to server to despawn the client
        /// Adds request to servers' pending despawn set
        /// </summary>
        /// <param name="rpcParams">Server RPC Params</param>
        [ServerRpc(RequireOwnership = false)]
        private void DespawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingDespawn.Add(senderClientId);
            //m_spawnManager.DespawnPlayer(senderClientId);
        }

        /// <summary>
        /// Sets up a network component for the provided actor entity.
        /// </summary>
        /// <param name="actor">Actor entity / gameobject</param>
        /// <exception cref="ArgumentNullException">actor cannot be null.</exception>
        public void SetupActorNetworkComponent(GameObject actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            var cmpActorNetwork = actor.GetComponent<ActorNetwork>();
            cmpActorNetwork.ActorSpawnManager = this;
        }

        /// <summary>
        /// Registers entity with input manager
        /// </summary>
        /// <param name="actor">Actor / entity to be registered</param>
        /// <exception cref="NullReferenceException">actor cannot be null</exception>
        public void RegisterActorOnInputManager(GameObject actor)
        {
            if (actor == null)
            {
                throw new NullReferenceException(nameof(actor));
            }

            var controller = actor.GetComponent<PlayerInput>();
            InputManager.Instance.RegisterPlayerController(controller);
        }

        /// <summary>
        /// Creates a camera for the provided actor / entity
        /// </summary>
        /// <param name="actor">Actor / Entity</param>
        /// <param name="isLocal">Flag indicating whether the player is local (to this machine) or not</param>
        /// <exception cref="ArgumentNullException">actor cannot be null</exception>
        ///  /// <exception cref="NullReferenceException">Camera manager has not been set</exception>
        public void CreateActorCamera(GameObject actor, bool isLocal)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (m_cameraManager == null)
            {
                throw new NullReferenceException("No 'CameraManager' has been set.");
            }

            var cameraSystem = m_cameraManager.GetComponent<CameraManager>();

            var cameraObj = GameObject.Instantiate(m_cameraPrefab);
            var actorCamera = cameraObj.GetComponent<ActorCamera>();
            actorCamera.Target = actor;

            cameraObj.AddComponent<Camera>();

            var camera = cameraObj.GetComponent<Camera>();

            actor.GetComponent<ActorFloating>().ActorCamera = actorCamera;
            actor.GetComponent<ActorGrounded>().ActorCamera = actorCamera;
            actor.GetComponent<ActorCrosshair>().ActorCamera = actorCamera;
            
            if (isLocal)
            {
                actorCamera.AddComponent<AudioListener>();
            }

            cameraSystem.AddCamera(camera, isLocal);
        }

        /// <summary>
        /// Assigns the actor of the UI Overlay.
        /// </summary>
        /// <param name="actor">actor entity to be assigned</param>
        /// <exception cref="ArgumentNullException">actor cannot be null</exception>
        public void SetupUIOverlay(GameObject actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            m_sceneSettings.UIGameOverlay.Actor = actor.GetComponent<ActorController>();
        }

        /// <summary>
        /// Fetches a spawn point at random for the given actor
        /// </summary>
        /// <param name="actor">actor / entity</param>
        /// <returns>A spawn point gameobject</returns>
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

        /// <summary>
        /// Prepares an actor for a local player.
        ///     -> Initialise 
        ///     -> Sets up network component
        ///     -> Adds player to input manager
        ///     -> Creates a camera for the player
        ///     -> Assigns the player to the overlay UI, so that it displays information relating to the current actor
        ///     -> Moves the player actor to the spawn point
        ///     -> Notifies any subscribers
        /// </summary>
        /// <param name="actor">The actor to be prepared following spawn</param>
        public void PrepareLocalPlayerActor(GameObject actor)
        {
            this.InitialiseBehaviours(actor);
            this.SetupActorNetworkComponent(actor);
            this.RegisterActorOnInputManager(actor);
            this.CreateActorCamera(actor, true);
            this.SetupUIOverlay(actor);

            this.MoveActorToSpawnPoint(actor);

            this.OnActorSpawn?.Invoke(this, new OnActorSpawnedArgs(actor));
        }

        /// <summary>
        /// Prepares an actor for a remote player.
        ///     -> Initialise 
        ///     -> Sets up network component
        ///     -> Creates a camera for the actor
        ///     -> Moves the actor to the spawn point
        ///     -> Notifies any subscribers
        /// </summary>
        /// <param name="actor">The actor to be prepared following spawn</param>
        public void PrepareRemotePlayerActor(GameObject actor)
        {
            this.InitialiseBehaviours(actor);
            this.SetupActorNetworkComponent(actor);
            this.CreateActorCamera(actor, false);
            this.MoveActorToSpawnPoint(actor);

            this.OnActorSpawn?.Invoke(this, new OnActorSpawnedArgs(actor));
        }

        /// <summary>
        /// Gets a spawn point and moves the provided actor to that spawn point.
        /// </summary>
        /// <param name="actor">actor / entity to be spawned</param>
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
