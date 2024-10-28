using Assets.Scripts.Actor;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Assets.Scripts.Pickups.Weapons
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
                m_wpnNetworkObjectId.Value = value;
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
                m_hitNetworkObjectId.Value = value;
            }
        }

        public ProjectileNetwork()
        {
            m_hitNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
            m_hitNetworkObjectId.OnValueChanged += OnHitNetworkObjectChanged;

            m_wpnNetworkObjectId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
            m_wpnNetworkObjectId.OnValueChanged += OnNetworkObjectValueChanged;
        }

        private void Awake()
        {
            m_projectile = this.GetComponent<Projectile>();
        }

        private void Start()
        {
            m_projectile = this.GetComponent<Projectile>();
        }

        private void Update()
        {
            
        }

        private void OnHitNetworkObjectChanged(ulong previousValue, ulong newValue)
        {
            if (newValue != 0)
            {
                var gameObj = this.NetworkManager.SpawnManager.SpawnedObjects[newValue];
                var weaponObj = this.NetworkManager.SpawnManager.SpawnedObjects[m_wpnNetworkObjectId.Value];

                var actorState = gameObj.GetComponent<ActorState>();

                if (actorState != null)
                {
                    actorState.LastHitBy = weaponObj.gameObject;
                }
            }
        }

        private void OnNetworkObjectValueChanged(ulong oldVal, ulong value)
        {
            var weaponObj = this.NetworkManager.SpawnManager.SpawnedObjects[value];
            m_projectile.Weapon = weaponObj.gameObject.GetComponent<Weapon>();
        }

        public void SetWeaponNetworkObjectId(ulong weaponObjId)
        {
            m_wpnNetworkObjectId.Value = weaponObjId;
            var weaponObj = this.NetworkManager.SpawnManager.SpawnedObjects[weaponObjId];
            m_projectile.Weapon = weaponObj.gameObject.GetComponent<Weapon>();
        }

        public override void OnNetworkSpawn()
        {
            //
        }
    }
}
