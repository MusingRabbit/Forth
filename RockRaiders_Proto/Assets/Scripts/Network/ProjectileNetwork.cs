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
        /// <summary>
        /// Weapon network object id
        /// </summary>
        private NetworkVariable<ulong> m_wpnNetworkObjectId;

        /// <summary>
        /// Hit network object id
        /// </summary>
        private NetworkVariable<ulong> m_hitNetworkObjectId;

        /// <summary>
        /// Projectile component
        /// </summary>
        private Projectile m_projectile;

        /// <summary>
        /// Gets or sets the weapon network object id
        /// </summary>
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
                    SetWeaponNetworkObjectIdServerRpc(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the network object id that is hit by the projectile
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectileNetwork()
        {
            m_hitNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
            m_wpnNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
        }

        /// <summary>
        /// Called on load
        /// </summary>
        private void Awake()
        {
            m_projectile = GetComponent<Projectile>();
        }

        /// <summary>
        /// Called before first frame
        /// </summary>
        private void Start()
        {
            m_projectile = GetComponent<Projectile>();
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Called when hit network object id has its value changed
        /// Performs a lookup for the game object that is hit and 
        /// sends out a player attacked notification if the object hit was a player
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        private void OnHitNetworkObjectChanged(ulong previousValue, ulong newValue)
        {
            if (newValue != 0)
            {
                NotificationService.Instance.Info($"WeaponNetworkObjectId : {m_wpnNetworkObjectId.Value} | HitNetworkObjectId : {newValue}");

                var spawnedObjects = NetworkManager.SpawnManager.SpawnedObjects;

                if (spawnedObjects.ContainsKey(newValue))
                {
                    var gameObj = spawnedObjects[newValue];
                    var weaponObj = spawnedObjects[m_wpnNetworkObjectId.Value];

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
                    NotificationService.Instance.Warning($"No spawned object for {newValue} exists");
                }
            }
            else
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Called when network object id has changed
        /// </summary>
        /// <param name="oldVal"></param>
        /// <param name="value"></param>
        private void OnNetworkObjectValueChanged(ulong oldVal, ulong value)
        {
            NotificationService.Instance.Info($"WeaponNetworkObjectId : {value}");
            SetWeaponNetworkObjectId(value);
        }

        /// <summary>
        /// Sets the weapon network object id
        /// Updates the assocaited weapon for the projectile
        /// </summary>
        /// <param name="weaponObjId">Weapon network object id</param>
        public void SetWeaponNetworkObjectId(ulong weaponObjId)
        {
            if (IsServer)
            {
                m_wpnNetworkObjectId.Value = weaponObjId;
            }

            var spawnedObjects = NetworkManager.SpawnManager.SpawnedObjects;

            if (spawnedObjects.ContainsKey(weaponObjId))
            {
                var weaponObj = spawnedObjects[weaponObjId];
                m_projectile.Weapon = weaponObj.gameObject.GetComponent<Weapon>();
            }
        }

        /// <summary>
        /// Server request to change hit network object id
        /// </summary>
        /// <param name="value"></param>
        [Rpc(SendTo.Server)]
        private void SetHitNetworkObjectIdServerRpc(ulong value)
        {
            m_hitNetworkObjectId.Value = value;
        }
        /// <summary>
        /// Client notification to update network object id
        /// </summary>
        /// <param name="value"></param>

        [Rpc(SendTo.ClientsAndHost)]
        private void SetHitNetworkObjectIdClientRpc(ulong value)
        {
            OnHitNetworkObjectChanged(m_hitNetworkObjectId.Value, value);
        }

        /// <summary>
        /// Server request to update weapon object id
        /// </summary>
        /// <param name="value"></param>
        [Rpc(SendTo.Server)]
        private void SetWeaponNetworkObjectIdServerRpc(ulong value)
        {
            m_wpnNetworkObjectId.Value = value;
        }

        /// <summary>
        /// Called when entity is spawned
        /// </summary>
        public override void OnNetworkSpawn()
        {
            m_hitNetworkObjectId.OnValueChanged += OnHitNetworkObjectChanged;
            m_wpnNetworkObjectId.OnValueChanged += OnNetworkObjectValueChanged;
        }
    }
}
