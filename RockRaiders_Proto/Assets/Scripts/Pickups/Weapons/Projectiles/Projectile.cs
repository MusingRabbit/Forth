using Assets.Scripts.Network;
using System;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        private Weapon m_weapon;

        [SerializeField]
        private TimeSpan m_lifeSpan;

        [SerializeField]
        private float m_mass;


        private float m_startTime;
        private float m_muzzleVelcity;

        private bool m_isDespawning;

        public float MuzzleVelocity
        {
            get
            {
                return m_muzzleVelcity;
            }
            set
            {
                m_muzzleVelcity = value;
            }
        }

        public float Mass
        {
            get
            {
                return m_mass;
            }
            set
            {
                m_mass = value;
            }
        }

        public Weapon Weapon
        {
            get
            {
                return m_weapon;
            }
            set
            {
                m_weapon = value;
            }
        }

        private Rigidbody m_rigidBody;
        private ProjectileSpawnManager m_projNetwork;

        private void Awake()
        {

        }


        // Start is called before the first frame update
        public virtual void Start()
        {
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_rigidBody.AddForce(transform.forward * m_muzzleVelcity, ForceMode.Impulse);
            m_rigidBody.mass = m_mass;
            m_startTime = Time.time;
            m_lifeSpan = TimeSpan.FromSeconds(5);

            m_projNetwork = this.GetComponent<ProjectileSpawnManager>();
        }

        // Update is called once per frame
        public virtual void Update()
        {
            var currSpan = Time.time - m_startTime;

            if (m_lifeSpan < TimeSpan.FromSeconds(currSpan))
            {
                //Destroy(this.gameObject);
                m_projNetwork.DespawnProjectile(gameObject);
                m_isDespawning = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            m_lifeSpan = TimeSpan.FromSeconds((Time.time - m_startTime) + 0.2f);
        }

        internal void Despawn(Collision collision)
        {
            if (!m_isDespawning)
            {
                m_projNetwork.DespawnProjectile(gameObject, collision);
                m_isDespawning = true;
            }
        }
    }
}