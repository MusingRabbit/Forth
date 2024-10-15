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
        private GameObject m_gameUIPrefab;

        [SerializeField]
        private GameObject m_cameraManagerObj;

        [SerializeField]
        private GameObject m_inputManagerObj;

        [SerializeField]
        private GameObject m_playerPrefab;

        [SerializeField]
        private List<GameObject> m_spawnPoints;
         
        public ActorSpawnManager()
        {
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

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = this;

            var spawnPoint = this.GetSpawnPoint(player);
            player.transform.position = spawnPoint.transform.position;

            // Get the NetworkObject component
            var playerNetworkObject = player.GetComponent<NetworkObject>();

            // Spawn the player object on all clients
            playerNetworkObject.SpawnAsPlayerObject(clientId);
        }

        public void AddActorNetworkComponent(GameObject actor)
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
            var controller = actor.GetComponent<PlayerController>();
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
            cameraObj.AddComponent<ActorCamera>();

            var tpc = cameraObj.GetComponent<ActorCamera>();
            tpc.Target = actor;
            tpc.Distance = 5.0f;
            tpc.Offset = new Vector3(0, 10.0f, 0);
            

            cameraObj.AddComponent<Camera>();

            var camera = cameraObj.GetComponent<Camera>();

            cameraSystem.AddCamera(camera, isLocal);
        }

        public void CreateActorUI(GameObject actor, bool isLocal)
        {
            if (m_gameUIPrefab == null)
            {
                throw new NullReferenceException("No 'Game UI Prefab' has been set.");
            }

            var cameraSystem = m_cameraManagerObj.GetComponent<CameraManager>();
            var gameUI = GameObject.Instantiate(m_gameUIPrefab);
            gameUI.AddComponent<FollowObject>();

            var followObj = gameUI.GetComponent<FollowObject>();
            followObj.Target = actor;
            followObj.Offset = new Vector3(0, 0, 0);
            followObj.IsLookingAtTarget = false;

            var actorController = actor.GetComponent<ActorController>();
            var crosshairObj = gameUI.FindChild("Crosshair"); 

            if (crosshairObj == null)
            {
                throw new NullReferenceException("Crosshair is null!");
            }

            //actorController.Crosshair = gameUI.FindChild("Crosshair");
            //var crosshair = actorController.Crosshair.GetComponent<Crosshair>();

            //if (isLocal)
            //{
            //    crosshair.Actor = actor;
            //    crosshair.Camera = cameraSystem.GetSelectedCamera();
            //    crosshair.PlayerController = actor.GetComponent<PlayerController>();
            //}
            //else
            //{
            //    crosshair.enabled = false;  
            //}

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
            this.AddActorNetworkComponent(actor);
            this.RegisterActorOnInputManager(actor);
            this.CreateActorCamera(actor, true);
            //this.CreateActorUI(actor, true);
        }

        public void PrepareRemotePlayerActor(GameObject actor)
        {
            this.AddActorNetworkComponent(actor);
            this.CreateActorCamera(actor, false);
            //this.CreateActorUI(actor, false);
        }
    }
}
