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
    /// <summary>
    /// Projectile spawn manager
    /// </summary>
    public class ProjectileSpawnManager : NetworkBehaviour
    {
        /// <summary>
        /// Projectile prefab
        /// </summary>
        [SerializeField]
        private GameObject m_projectilePrefab;

        /// <summary>
        /// Projectile component
        /// </summary>
        private Projectile m_projectile;

        /// <summary>
        /// Gets the projectile prefabs' projectile component
        /// </summary>
        public Projectile Projectile
        {
            get
            {
                return m_projectile;
            }
        }

        /// <summary>
        /// Called before first frame
        /// </summary>
        private void Start()
        {
            m_projectile = m_projectilePrefab.GetComponent<Projectile>();
        }

        /// <summary>
        /// Spawn a projectile
        /// </summary>
        /// <param name="weaponObj">Weapon that is spawning the projectile</param>
        /// <param name="position">Position to spawn the projectile at</param>
        /// <param name="rotation">Rotation of the projectile</param>
        /// <param name="velocityOffset">Velocity offset (recoil/current velocity)</param>
        /// <param name="muzzleVelocity">Velocity</param>
        /// <returns></returns>
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

        /// <summary>
        /// Despawns projectile
        /// </summary>
        /// <param name="projectile">Projectile to be removed</param>
        /// <param name="collision">The collision responsible for removing this projectile</param>
        /// <exception cref="InvalidOperationException">No NetworkObject component could be found</exception>
        public void DespawnProjectile(GameObject projectile, Collision collision)
        {
            var networkObject = projectile.GetComponent<NetworkObject>();
            var contact = collision.GetContact(0);
            var hitObject = contact.thisCollider.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                throw new InvalidOperationException("Projectile does not have a NetworkObject.");
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

        /// <summary>
        /// Despawns projectile
        /// </summary>
        /// <param name="projectile">Projectile to despawn</param>
        /// <exception cref="InvalidOperationException">Projectile does not have a NetworkObject component</exception>
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

        /// <summary>
        /// Server request to despawn projectile
        /// </summary>
        /// <param name="networkObjId">Projectile network object id</param>
        /// <param name="contactNetworkObjId">Network object id of object projectile has made contact with</param>
        [ServerRpc]
        private void DespawnProjectileServerRpc(ulong networkObjId, ulong contactNetworkObjId)
        {
            NotificationService.Instance.Info($"{networkObjId}|{contactNetworkObjId}");

            if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjId))
            {
                var networkObject = GetNetworkObject(networkObjId);

                if (networkObject != null)
                {
                    var projectileNet = networkObject.GetComponent<ProjectileNetwork>();
                    projectileNet.HitNetworkObjectId = contactNetworkObjId;

                    networkObject.Despawn(true);
                }
                else
                {
                    NotificationService.Instance.Warning($"No network object could be found for id : {NetworkObjectId}");
                }
            }
        }

        /// <summary>
        /// Server request to despawn projectile
        /// </summary>
        /// <param name="networkObjId">projectile network object id</param>
        /// <exception cref="NullReferenceException"></exception>
        [ServerRpc]
        private void DespawnProjectileServerRpc(ulong networkObjId)
        {
            NotificationService.Instance.Info(networkObjId);

            if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjId))
            {
                var networkObject = GetNetworkObject(networkObjId);

                if (networkObject != null)
                {
                    networkObject.Despawn(true);
                }
                else
                {
                    NotificationService.Instance.Warning($"No network object could be found for id : {NetworkObjectId}");
                }
            }
        }

        /// <summary>
        /// Instantiates projectile
        /// </summary>
        /// <param name="weapon">Weapon assocaited with this projectile</param>
        /// <param name="position">Position that the projectile is to be spawned at</param>
        /// <param name="rotation">Rotation of the projectile</param>
        /// <param name="velocityOffset">Velocity offset of the projectile</param>
        /// <param name="muzzleVelocity">Velocity of the projectile</param>
        /// <returns></returns>
        private GameObject CreateProjectile(Weapon weapon, Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            var instance = GameObject.Instantiate(m_projectilePrefab);

            var rigidBody = instance.GetComponent<Rigidbody>();
            var projectile = instance.GetComponent<Projectile>();

            NotificationService.Instance.Info(weapon.name);

            projectile.Weapon = weapon.GetComponent<Weapon>();
            projectile.MuzzleVelocity = muzzleVelocity;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            projectile.AddAdditionalForce(velocityOffset);
            instance.SetActive(true);

            return instance;
        }

        /// <summary>
        /// Server request to spawn projectile
        /// </summary>
        /// <param name="wpnNetObjId">Weapon network object id</param>
        /// <param name="position">Position to spawn projectile at</param>
        /// <param name="rotation">Rotation to orient to projectile to</param>
        /// <param name="velocityOffset">Veloctiy offset</param>
        /// <param name="muzzleVelocity">Velocity</param>

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
        }
    }
}
