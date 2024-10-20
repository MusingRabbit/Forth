using Assets.Scripts.Actor;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private GameObject m_inputManagerObj;

        [SerializeField]
        private GameObject m_playerPrefab;

        [SerializeField]
        private List<GameObject> m_spawnPoints;

        [SerializeField]
        private UIGameOverlay m_uiOverlay;

        public ActorSpawnManager()
        {
            instance = this;
            m_spawnPoints = new List<GameObject>();
        }

        public void Start()
        {

        }

        public void Update()
        {
        }

        public override void OnNetworkSpawn()
        {
            this.SpawnPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;

            SpawnPlayerOnClients(senderClientId);
        }

        private void SpawnPlayerOnClients(ulong clientId)
        {
            Debug.Log($"SpawnPlayerOnClients - clientId: {clientId}");

            // Instantiate player object
            GameObject player = Instantiate(m_playerPrefab);

            this.InitialiseBehaviours(player);

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = this;

            var spawnPoint = this.GetSpawnPoint(player);
            player.transform.position = spawnPoint.transform.position;

            // Get the NetworkObject component
            var playerNetworkObject = player.GetComponent<NetworkObject>();

            // Spawn the player object on all clients
            playerNetworkObject.SpawnAsPlayerObject(clientId);
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

            if (m_inputManagerObj == null)
            {
                throw new NullReferenceException("No 'InputManager' has been set.");
            }

            var inputManager = m_inputManagerObj.GetComponent<InputManager>();
            var controller = actor.GetComponent<PlayerInput>();
            inputManager.RegisterPlayerController(controller);
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
            var spawmPoints = m_spawnPoints.Select(x => x.GetComponent<SpawnPoint>()).Where(x => x.Team == controller.Team).ToList();
            var rndIdx = Random.Range(0, spawmPoints.Count - 1);
            return spawmPoints[rndIdx].gameObject;
        }

        public void PrepareLocalPlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.RegisterActorOnInputManager(actor);
            this.CreateActorCamera(actor, true);
            this.SetupUIOverlay(actor);
        }

        public void PrepareRemotePlayerActor(GameObject actor)
        {
            this.SetupActorNetworkComponent(actor);
            this.CreateActorCamera(actor, false);
        }
    }
}
