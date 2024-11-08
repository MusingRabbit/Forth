using Assets.Scripts.Managers;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class ActorSpawnManagerNetwork : NetworkBehaviour
    {
        private ActorSpawnManager m_spawnManager;
        private HashSet<ulong> m_pendingSpawn;
        private HashSet<ulong> m_pendingDespawn;

        private Dictionary<ulong, NetworkObject> m_clients;

        public ActorSpawnManager SpawnManager
        {
            get
            {
                return m_spawnManager;
            }
            set
            {
                m_spawnManager = value;
            }
        }

        public ActorSpawnManagerNetwork()
        {
            m_pendingSpawn = new HashSet<ulong>();
            m_pendingDespawn = new HashSet<ulong>();
            m_clients = new Dictionary<ulong, NetworkObject>();
        }


        private void Start()
        {
            m_spawnManager = this.GetComponent<ActorSpawnManager>();

            //m_spawnManager.OnServerSpawnPlayerCalled += this.SpawnManager_OnSpawnPlayerCalled;
            //m_spawnManager.OnServerDespawnPlayerCalled += this.SpawnManager_OnDespawnPlayerCalled;
        }

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

        private void InitialiseBehaviours(GameObject actor)
        {
            var behaviours = actor.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                behaviour.Initialise();
            }
        }

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

        [ServerRpc(RequireOwnership = false)]
        public void SpawnClientServerRpc(ulong clientId)
        {
            m_pendingSpawn.Add(clientId);
            //m_spawnManager.SpawnPlayer(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnClientServerRpc(ulong clientId)
        {
            m_pendingDespawn.Add(clientId);
            //m_spawnManager.DespawnPlayer(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingSpawn.Add(senderClientId);
            //m_spawnManager.SpawnPlayer(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnClientServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            m_pendingDespawn.Add(senderClientId);
            //m_spawnManager.DespawnPlayer(senderClientId);
        }

        private void SpawnManager_OnDespawnPlayerCalled(object sender, EventArgs e)
        {
            this.DespawnClientServerRpc();
        }

        private void SpawnManager_OnSpawnPlayerCalled(object sender, EventArgs e)
        {
            this.SpawnClientServerRpc();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }
    }
}
