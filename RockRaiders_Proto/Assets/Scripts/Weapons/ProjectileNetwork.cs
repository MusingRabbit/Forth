using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Weapons
{

    public class ProjectileNetwork : NetworkBehaviour
    {
        [SerializeField]
        private GameObject m_projectilePrefab;

        private static HashSet<Type> RegisteredPrefabs = new HashSet<Type>();


        private void Start()
        {

            //if (m_projectilePrefab != null && !RegisteredPrefabs.Contains(m_projectilePrefab.GetType()))
            //{
            //    RegisteredPrefabs.Add(m_projectilePrefab.GetType());
            //    this.NetworkManager.AddNetworkPrefab(m_projectilePrefab);
            //}
        }

        public void SpawnProjectile(Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            this.SpawnProjectileServerRpc(position, rotation, velocityOffset, muzzleVelocity);
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

        private GameObject CreateProjectile(Vector3 position, Quaternion rotation, Vector3 currVelocity, float muzzleVelocity)
        {
            var instance = Instantiate(m_projectilePrefab);

            var rigidBody = instance.GetComponent<Rigidbody>();
            var projectile = instance.GetComponent<Projectile>();

            projectile.MuzzleVelocity = muzzleVelocity;
            projectile.Mass = 1.0f;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            rigidBody.isKinematic = false;
            rigidBody.velocity += currVelocity;
            instance.SetActive(true);

            return instance;
        }


        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector3 position, Quaternion rotation, Vector3 velocityOffset, float muzzleVelocity)
        {
            var projectile = this.CreateProjectile(position, rotation, velocityOffset, muzzleVelocity);
            var netObj = projectile.GetComponent<NetworkObject>();
            netObj.Spawn();

            //SpawnProjectileClientRpc(position, rotation, currVelocity, muzzleVelocity);
        }

        [ClientRpc]
        private void SpawnProjectileClientRpc(Vector3 position, Quaternion rotation, Vector3 currVelocity, float muzzleVelocity)
        {
            if (this.IsOwner)
            {
                return;
            }

            this.CreateProjectile(position, rotation, currVelocity, muzzleVelocity);
        }
    }
}
