using Assets.Scripts.Level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;


namespace Assets.Scripts.Network
{
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

    public class FlagNetwork : NetworkBehaviour
    {
        private Flag m_flag;
        private NetworkVariable<FlagStateData> m_flagState;

        public FlagNetwork()
        {
            m_flagState = new NetworkVariable<FlagStateData>(writePerm: NetworkVariableWritePermission.Server);
        }

        private void Start()
        {
            m_flag = this.GetComponent<Flag>();
        }

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

        private void UpdateFlagState()
        {
            var state = m_flagState.Value;
            var spawnedObjs = NetworkManager.SpawnManager.SpawnedObjects;

            NetworkObject owner = null;

            if (state.OwnerNetworkId > 0 && spawnedObjs.ContainsKey(state.OwnerNetworkId))
            {
                owner = NetworkManager.SpawnManager.SpawnedObjects[state.OwnerNetworkId];
            }

            m_flag.Team = state.Team;
            m_flag.Captured = state.Captured;
            m_flag.Retreived = state.Retreived;

            if (owner != null)
            {
                m_flag.Owner = owner.gameObject;
            }
        }

        [Rpc(SendTo.Server)]
        private void TransmitFlagStateServerRpc(FlagStateData data)
        {
            m_flag.Team = data.Team;
        }
    }
}