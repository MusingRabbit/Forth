using Assets.Scripts.Actor;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using Assets.Scripts.Services;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts.Network
{
    public class ProjectileSpawnManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject m_projectilePrefab;


        private Projectile m_projectile;

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

        }

        public bool SpawnProjectile(GameObject weaponObj, Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            if (IsOwner)
            {
                var wpnnetObjId = weaponObj.GetComponent<NetworkObject>().NetworkObjectId;
                SpawnProjectileServerRpc(wpnnetObjId, position, rotation, velocityOffset, muzzleVelocity);
                return true;
            }

            return false;
        }

        public void DespawnProjectile(GameObject projectile, Collision collision)
        {
            var networkObject = projectile.GetComponent<NetworkObject>();
            var contact = collision.GetContact(0);
            var hitObject = contact.thisCollider.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                throw new InvalidOperationException("Projectile does not have a NetworkComponent.");
            }

            if (!IsOwner)
            {
                return;
            }

            if (hitObject == null)
            {
                DespawnProjectileServerRpc(networkObject.NetworkObjectId);
            }
            else
            {
                DespawnProjectileServerRpc(networkObject.NetworkObjectId, hitObject.NetworkObjectId);
            }
        }

        public void DespawnProjectile(GameObject projectile)
        {
            var networkObject = projectile.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                throw new InvalidOperationException("Projectile does not have a NetworkComponent.");
            }

            if (!IsOwner)
            {
                return;
            }

            DespawnProjectileServerRpc(networkObject.NetworkObjectId);

        }

        [ServerRpc]
        private void DespawnProjectileServerRpc(ulong networkObjId, ulong contactNetworkObjId)
        {
            NotificationService.Instance.Info($"{networkObjId}|{contactNetworkObjId}");

            if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjId))
            {
                var networkObject = GetNetworkObject(networkObjId);

                if (networkObject == null)
                {
                    NotificationService.Instance.Warning($"No network object could be found for id : {NetworkObjectId}");
                }
                else
                {
                    var projectileNet = networkObject.GetComponent<ProjectileNetwork>();
                    projectileNet.HitNetworkObjectId = contactNetworkObjId;

                    networkObject.Despawn(true);
                }
            }
        }

        [ServerRpc]
        private void DespawnProjectileServerRpc(ulong networkObjId)
        {
            NotificationService.Instance.Info(networkObjId);

            if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjId))
            {
                var networkObject = GetNetworkObject(networkObjId);

                if (networkObject == null)
                {
                    throw new NullReferenceException($"No network object could be found for id : {NetworkObjectId}");
                }

                networkObject.Despawn(true);
            }
        }

        private GameObject CreateProjectile(Weapon weapon, Vector3 position, Quaternion rotation, Vector3 currVelocity, float muzzleVelocity)
        {
            var instance = GameObject.Instantiate(m_projectilePrefab);

            var rigidBody = instance.GetComponent<Rigidbody>();
            var projectile = instance.GetComponent<Projectile>();

            NotificationService.Instance.Info(weapon.name);

            projectile.Weapon = weapon.GetComponent<Weapon>();
            projectile.MuzzleVelocity = muzzleVelocity;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            rigidBody.velocity += currVelocity;
            instance.SetActive(true);

            return instance;
        }


        [ServerRpc]
        private void SpawnProjectileServerRpc(ulong wpnNetObjId, Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            var wpnObj = GetNetworkObject(wpnNetObjId);
            var weapon = wpnObj.gameObject.GetComponent<Weapon>();

            NotificationService.Instance.Info("Weapon Name : " + wpnObj.name);

            var projectile = CreateProjectile(weapon, position, rotation, velocityOffset, muzzleVelocity);
            var damage = projectile.GetComponent<Damage>();
            damage.Base = weapon.Damage;

            var netObj = projectile.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            var projNet = projectile.GetComponent<ProjectileNetwork>();
            projNet.WeaponNetworkObjectId = wpnNetObjId;

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
