using Assets.Scripts.Level;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;


namespace Assets.Scripts.Network
{
    /// <summary>
    /// Flag state network data
    /// </summary>
    public struct FlagStateData : INetworkSerializable
    {
        public Team Team;
        public ulong OwnerNetworkId;
        public bool Captured;
        public bool Retreived;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.Team);
            serializer.SerializeValue(ref this.OwnerNetworkId);
            serializer.SerializeValue(ref this.Captured);
            serializer.SerializeValue(ref this.Retreived);
        }
    }

    /// <summary>
    /// Flag network
    /// </summary>
    public class FlagNetwork : NetworkBehaviour
    {
        /// <summary>
        /// Flag component
        /// </summary>
        private Flag m_flag;

        /// <summary>
        /// Flag state network data
        /// </summary>
        private NetworkVariable<FlagStateData> m_flagState;

        /// <summary>
        /// Constructor
        /// </summary>
        public FlagNetwork()
        {
            m_flagState = new NetworkVariable<FlagStateData>(writePerm: NetworkVariableWritePermission.Server);
        }

        /// <summary>
        /// Called before first frame
        /// </summary>
        private void Start()
        {
            m_flag = this.GetComponent<Flag>();
        }

        /// <summary>
        /// Called every frame
        /// Syncronises flag state accross network
        /// </summary>
        private void Update()
        {
            if (IsOwner)
            {
                SendState();
            }
            else
            {
                UpdateFlagState();
            }
        }

        /// <summary>
        /// Sends state to network
        /// </summary>
        private void SendState()
        {
            ulong ownerNetId = 0;

            if (m_flag.Owner != null)
            {
                ownerNetId = m_flag.Owner.GetComponent<NetworkObject>().NetworkObjectId;
            }

            var state = new FlagStateData { Team = m_flag.Team, OwnerNetworkId = ownerNetId, Captured = m_flag.Captured };

            if (IsServer)
            {
                m_flagState.Value = state;
            }
            else
            {
                this.TransmitFlagStateServerRpc(state);
            }
        }

        /// <summary>
        /// Updates flag state from network
        /// </summary>
        private void UpdateFlagState()
        {
            var state = m_flagState.Value;
            var spawnedObjs = NetworkManager.SpawnManager.SpawnedObjects;

            NetworkObject owner = null;

            if (state.OwnerNetworkId > 0)
            {
                if (spawnedObjs.ContainsKey(state.OwnerNetworkId))
                {
                    owner = spawnedObjs[state.OwnerNetworkId];
                }
                else
                {
                    NotificationService.Instance.Warning($"No spawned object for Id '{state.OwnerNetworkId}' could be found.");
                }
            }

            m_flag.Team = state.Team;
            m_flag.Captured = state.Captured;
            m_flag.Retreived = state.Retreived;

            if (owner != null)
            {
                m_flag.Owner = owner.gameObject;
            }
        }

        /// <summary>
        /// Server request to update network flag state
        /// </summary>
        /// <param name="data">Request data</param>
        [Rpc(SendTo.Server)]
        private void TransmitFlagStateServerRpc(FlagStateData data)
        {
            m_flag.Team = data.Team;
        }
    }
}