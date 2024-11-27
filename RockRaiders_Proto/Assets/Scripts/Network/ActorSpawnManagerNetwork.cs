using Assets.Scripts.Managers;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// Actor network spawn manager
    /// </summary>
    public class ActorSpawnManagerNetwork : NetworkBehaviour
    {
        /// <summary>
        /// Reference to spawn manager
        /// </summary>
        private ActorSpawnManager m_spawnManager;

        /// <summary>
        /// Set of all network objects pending spawn
        /// </summary>
        private HashSet<ulong> m_pendingSpawn;

        /// <summary>
        /// Set of all network objects pending despawn
        /// </summary>
        private HashSet<ulong> m_pendingDespawn;

        /// <summary>
        /// Dictionary of all clients
        /// </summary>
        private Dictionary<ulong, NetworkObject> m_clients;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorSpawnManagerNetwork()
        {
            m_pendingSpawn = new HashSet<ulong>();
            m_pendingDespawn = new HashSet<ulong>();
            m_clients = new Dictionary<ulong, NetworkObject>();
        }

        /// <summary>
        /// Called before first frame in scene
        /// </summary>
        private void Start()
        {
            m_spawnManager = this.GetComponent<ActorSpawnManager>();
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            if (this.IsServer)
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
        /// Spawns client by client id
        /// </summary>
        /// <param name="clientId">client id</param>
        public void SpawnClient(ulong clientId)
        {
            if (!this.IsServer)
            {
                this.SpawnClientServerRpc(clientId);
                return;
            }

            NotificationService.Instance.Info($"ClientId: {clientId}");

            // Instantiate player object
            GameObject player = GameObject.Instantiate(m_spawnManager.PlayerPrefab);

            this.InitialiseBehaviours(player);

            var actorNetwork = player.GetComponent<ActorNetwork>();
            actorNetwork.ActorSpawnManager = m_spawnManager;

            var playerNet = player.GetComponent<NetworkObject>();

            playerNet.SpawnAsPlayerObject(clientId, true);

            m_clients[clientId] = playerNet;
        }

        /// <summary>
        /// Initialises behaviours
        /// </summary>
        /// <param name="actor">Actor</param>
        private void InitialiseBehaviours(GameObject actor)
        {
            var behaviours = actor.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                behaviour.Initialise();
            }
        }

        /// <summary>
        /// Despawns client
        /// </summary>
        /// <param name="clientId">client id</param>
        public void DespawnClient(ulong clientId)
        {
            if (!this.IsServer)
            {
                this.DespawnClientServerRpc(clientId);
                return;
            }

            NotificationService.Instance.Info($"ClientId: {clientId}");

            if (m_clients.ContainsKey(clientId))
            {
                var playerNetworkObject = m_clients[clientId];

                if (playerNetworkObject != null)
                {
                    playerNetworkObject.Despawn(true);
                }

                m_clients.Remove(clientId);
            }
        }

        /// <summary>
        /// Respawns all clients within member client list
        /// </summary>
        public void RespawnAllClients()
        {
            if (this.IsServer)
            {
                foreach (var clientId in m_clients.Keys)
                {
                    this.DespawnClient(clientId);
                    this.SpawnClient(clientId);
                }
            }
        }

        /// <summary>
        /// Server request to spawn client by client id
        /// </summary>
        /// <param name="clientId"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SpawnClientServerRpc(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
        }

        /// <summary>
        /// Server request to despawn client by client id
        /// </summary>
        /// <param name="clientId"></param>
        [ServerRpc(RequireOwnership = false)]
        public void DespawnClientServerRpc(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
        }

        /// <summary>
        /// Server request to spawn client 
        /// </summary>
        /// <param name="rpcParams"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SpawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingSpawn.Add(senderClientId);
        }

        /// <summary>
        /// Server request to despawn client
        /// </summary>
        /// <param name="rpcParams"></param>
        [ServerRpc(RequireOwnership = false)]
        public void DespawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingDespawn.Add(senderClientId);
        }
    }
}
