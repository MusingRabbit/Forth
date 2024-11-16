using Assets.Scripts.Actor;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;

namespace Assets.Scripts.Network
{
    internal class ProjectileNetwork : NetworkBehaviour
    {
        private NetworkVariable<ulong> m_wpnNetworkObjectId;
        private NetworkVariable<ulong> m_hitNetworkObjectId;
        private Projectile m_projectile;

        public ulong WeaponNetworkObjectId
        {
            get
            {
                return m_wpnNetworkObjectId.Value;
            }
            set
            {
                if (IsServer)
                {
                    m_wpnNetworkObjectId.Value = value;
                }
                else
                {
                    SetHitWeaponNetworkObjectIdServerRpc(value);
                }
            }
        }

        public ulong HitNetworkObjectId
        {
            get
            {
                return m_hitNetworkObjectId.Value;
            }
            set
            {
                var strClient = IsServer ? "Server" : "Client";
                NotificationService.Instance.Info($"{strClient}|" + value.ToString());

                if (IsServer)
                {
                    m_hitNetworkObjectId.Value = value;
                    SetHitNetworkObjectIdClientRpc(value);
                }
                else
                {
                    SetHitNetworkObjectIdServerRpc(value);
                }
            }
        }

        public ProjectileNetwork()
        {
            m_hitNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
            m_wpnNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
        }

        private void Awake()
        {
            m_projectile = GetComponent<Projectile>();
        }

        private void Start()
        {
            m_projectile = GetComponent<Projectile>();
        }

        private void Update()
        {

        }

        private void OnHitNetworkObjectChanged(ulong previousValue, ulong newValue)
        {
            if (newValue != 0)
            {
                NotificationService.Instance.Info($"WeaponNetworkObjectId : {m_wpnNetworkObjectId.Value} | HitNetworkObjectId : {newValue}");

                var gameObj = NetworkManager.SpawnManager.SpawnedObjects[newValue];
                var weaponObj = NetworkManager.SpawnManager.SpawnedObjects[m_wpnNetworkObjectId.Value];

                NotificationService.Instance.Info($"{gameObj} | {weaponObj}");

                var actorState = gameObj.GetComponent<ActorState>();

                if (actorState != null)
                {
                    NotificationService.Instance.Info($"{gameObj} last hit by {actorState.LastHitBy}");
                    actorState.LastHitBy = weaponObj.gameObject;

                    NotificationService.Instance.NotifyPlayerAttacked(gameObj.gameObject);
                }
            }
            else
            {
                Debugger.Break();
            }
        }

        private void OnNetworkObjectValueChanged(ulong oldVal, ulong value)
        {
            NotificationService.Instance.Info($"WeaponNetworkObjectId : {value}");
            SetWeaponNetworkObjectId(value);
        }

        public void SetWeaponNetworkObjectId(ulong weaponObjId)
        {
            if (IsServer)
            {
                m_wpnNetworkObjectId.Value = weaponObjId;
            }

            var weaponObj = NetworkManager.SpawnManager.SpawnedObjects[weaponObjId];
            m_projectile.Weapon = weaponObj.gameObject.GetComponent<Weapon>();
        }

        [Rpc(SendTo.Server)]
        private void SetHitNetworkObjectIdServerRpc(ulong value)
        {
            m_hitNetworkObjectId.Value = value;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SetHitNetworkObjectIdClientRpc(ulong value)
        {
            OnHitNetworkObjectChanged(m_hitNetworkObjectId.Value, value);
        }

        [Rpc(SendTo.Server)]
        private void SetHitWeaponNetworkObjectIdServerRpc(ulong value)
        {
            m_wpnNetworkObjectId.Value = value;
        }

        public override void OnNetworkSpawn()
        {
            m_hitNetworkObjectId.OnValueChanged += OnHitNetworkObjectChanged;
            m_wpnNetworkObjectId.OnValueChanged += OnNetworkObjectValueChanged;
        }
    }
}
