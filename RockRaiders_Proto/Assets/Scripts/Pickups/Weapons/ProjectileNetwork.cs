using Assets.Scripts.Pickups.Weapons.Projectiles;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons
{

    public class ProjectileNetwork : NetworkBehaviour
    {
        [SerializeField]
        private GameObject m_projectilePrefab;

        private Projectile m_projectile;
        private Weapon m_weapon;

        public Projectile Projectile
        {
            get
            {
                return m_projectile;
            }
        }

        private void Start()
        {
            m_projectile = m_projectilePrefab.GetComponent<Projectile>();
            m_weapon = this.gameObject.GetComponent<Weapon>();
        }

        public void SpawnProjectile(Weapon weapon, Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            if (this.IsOwner)
            {
                var wpnnetObjId = weapon.GetComponent<NetworkObject>().NetworkObjectId;
                this.SpawnProjectileServerRpc(wpnnetObjId, position, rotation, velocityOffset, muzzleVelocity);
            }
        }

        public void DespawnProjectile(GameObject projectile)
        {
            var networkObject = projectile.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                throw new InvalidOperationException("Projectile does not have a NetworkComponent.");
            }

            if (!this.IsOwner)
            {
                return;
            }

            this.DespawnProjectileServerRpc(networkObject.NetworkObjectId);
        }

        [ServerRpc]
        private void DespawnProjectileServerRpc(ulong networkObjId)
        {
            var networkObject = this.GetNetworkObject(networkObjId);

            if (networkObject == null)
            {
                throw new NullReferenceException($"No network object could be found for id : {NetworkObjectId}");
            }

            networkObject.Despawn(true);
        }

        private GameObject CreateProjectile(Weapon weapon, Vector3 position, Quaternion rotation, Vector3 currVelocity, float muzzleVelocity)
        {
            var instance = Instantiate(m_projectilePrefab);

            var rigidBody = instance.GetComponent<Rigidbody>();
            var projectile = instance.GetComponent<Projectile>();

            projectile.Weapon = weapon;
            projectile.MuzzleVelocity = muzzleVelocity;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            rigidBody.isKinematic = false;
            rigidBody.velocity += currVelocity;
            instance.SetActive(true);

            return instance;
        }


        [ServerRpc]
        private void SpawnProjectileServerRpc(ulong wpnNetObjId, Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            var wpnObj = this.GetNetworkObject(wpnNetObjId);
            var weapon = wpnObj.gameObject.GetComponent<Weapon>();
            var projectile = this.CreateProjectile(weapon, position, rotation, velocityOffset, muzzleVelocity);
            var netObj = projectile.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            //SpawnProjectileClientRpc(position, rotation, currVelocity, muzzleVelocity);
        }

        //[ClientRpc]
        //private void SpawnProjectileClientRpc(Vector3 position, Quaternion rotation, Vector3 currVelocity, float muzzleVelocity)
        //{
        //    if (this.IsOwner)
        //    {
        //        return;
        //    }

        //    this.CreateProjectile(position, rotation, currVelocity, muzzleVelocity);
        //}
    }
}
