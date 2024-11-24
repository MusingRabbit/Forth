using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public struct PickupItemData : INetworkSerializable
    {
        public ulong OwnerNetworkObjectId;
        public ulong PickupNetworkObjectId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.OwnerNetworkObjectId);
            serializer.SerializeValue(ref this.PickupNetworkObjectId);
        }
    }

    public class PickupNetwork : NetworkBehaviour
    {
        private NetworkObject m_netObj;
        private PickupItem m_pickupItem;
        private Timer m_despawnTimer;

        public PickupNetwork()
        {
            
        }

        private void Start()
        {
            m_pickupItem = this.GetComponent<PickupItem>();
            m_netObj = this.GetComponent<NetworkObject>();

            m_pickupItem.OnPickedUp += PickupItem_OnPickedUp;
            m_pickupItem.OnDropped += PickupItem_OnDropped;

            if (this.IsServer)
            {
                if (m_pickupItem.SelfDespawnEnabled)
                {
                    m_despawnTimer = new Timer(TimeSpan.FromSeconds(60));
                    m_despawnTimer.OnTimerElapsed += this.DespawnTimer_OnTimerElapsed;
                    m_despawnTimer.Start();
                }
            }
        }

        private void PickupItem_OnDropped(object sender, EventArgs e)
        {
            if (this.IsServer)
            {
                var owner = m_pickupItem.Owner;

                if (owner != null)
                {
                    var ownerNetObj = owner.GetComponent<NetworkObject>();
                    var data = new PickupItemData { OwnerNetworkObjectId = ownerNetObj.NetworkObjectId };
                    this.TransmitPickupItemDataClientRpc(data);
                }
            }
            else if (this.IsClient)
            {
                var owner = m_pickupItem.Owner;

                if (owner != null)
                {
                    var ownerNetObj = owner.GetComponent<NetworkObject>();
                    var data = new PickupItemData { OwnerNetworkObjectId = ownerNetObj.NetworkObjectId };
                    this.TransmitPickupItemDroppedServerRpc(data);
                }
            }
        }

        private void PickupItem_OnPickedUp(object sender, EventArgs e)
        {
            if (this.IsServer)
            {
                var owner = m_pickupItem.Owner;
                var pickup = m_pickupItem;
                var ownerNetObj = owner.GetComponent<NetworkObject>();
                var pickupNetObj = pickup.GetComponent<NetworkObject>();

                var data = new PickupItemData { OwnerNetworkObjectId = ownerNetObj.NetworkObjectId, PickupNetworkObjectId = pickupNetObj.NetworkObjectId };
                this.TransmitPickupItemDataClientRpc(data);
            }
        }

        [Rpc(SendTo.Server)]
        private void TransmitPickupItemDroppedServerRpc(PickupItemData data)
        {
            this.TransmitPickupItemDataClientRpc(data);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TransmitPickupItemDataClientRpc(PickupItemData data)
        {
            if (this.IsClient)
            {
                this.SetActorPickupFromData(data);
            }
        }

        private void SetActorPickupFromData(PickupItemData data)
        {
            GameObject owner = null;
            GameObject item = null;

            var spawnedObjects = NetworkManager.SpawnManager.SpawnedObjects;

            if (spawnedObjects.ContainsKey(data.OwnerNetworkObjectId))
            {
                owner = spawnedObjects[data.OwnerNetworkObjectId].gameObject;
            }
            else
            {
                NotificationService.Instance.Warning($"No spawned object could be found for {data.OwnerNetworkObjectId}");
                return;
            }

            if (data.PickupNetworkObjectId > 0)
            {
                if (spawnedObjects.ContainsKey(data.PickupNetworkObjectId))
                {
                    item = spawnedObjects[data.PickupNetworkObjectId].gameObject;
                }
                else
                {
                    NotificationService.Instance.Warning($"No spawned pickup object could be found for {data.PickupNetworkObjectId}");
                    return;
                }
            }

            var actorPickup = owner.GetComponent<ActorPickup>();

            Weapon pickupWeapon = null;
            Flag flag = null;

            if (actorPickup == null)
            {
                return;
            }

            if (item != null)
            {
                pickupWeapon = item.GetComponent<Weapon>();
                flag = item.GetComponent<Flag>();

                if (pickupWeapon != null)
                {
                    if (pickupWeapon != null)
                    {
                        actorPickup.PickupWeapon(pickupWeapon, true);
                    }
                }

                if (flag != null)
                {
                    actorPickup.PickupPack(flag, true);
                }
            }
            else
            {
                actorPickup.DropSelectedWeapon(true);
            }


        }

        private void Update()
        {
            if (this.IsServer)
            {
                if (m_pickupItem.SelfDespawnEnabled && m_pickupItem.Owner == null)
                {
                    m_despawnTimer.Tick();
                }
            }
        }

        private void DespawnTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            if (this.IsServer)
            {
                m_netObj.Despawn(true);
            }
        }
    }
}
