using Assets.Scripts.HealthSystem;
using Assets.Scripts.Network;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private Weapon m_weapon;

        [SerializeField]
        private TimeSpan m_lifeSpan;

        [SerializeField]
        private float m_deactivationTime;

        private Timer m_deleteTimer;

        [SerializeField]
        private float m_mass;

        private List<Vector3> m_additionalForces;

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

        [SerializeField]
        private GameObject m_renderables;

        [SerializeField]
        private ParticleSystem m_particleSystem;

        private CapsuleCollider m_collider;
        private Explosive m_explosive;
        private Damage m_damage;
        private Rigidbody m_rigidBody;
        private ProjectileSpawnManager m_projNetwork;

        public Projectile()
        {
            m_additionalForces = new List<Vector3>();
        }

        private void Awake()
        {

        }


        // Start is called before the first frame update
        public virtual void Start()
        {
            m_collider = this.GetComponent<CapsuleCollider>();

            m_explosive = this.GetComponent<Explosive>();
            m_damage = this.GetComponent<Damage>();

            if (m_explosive != null)
            {
                m_explosive.Damage = m_damage;
            }

            m_rigidBody = this.GetComponent<Rigidbody>();
            m_rigidBody.mass = m_mass;
            m_rigidBody.AddForce(transform.forward.normalized * m_muzzleVelcity, ForceMode.Impulse);
            
            m_startTime = Time.time;
            m_lifeSpan = TimeSpan.FromSeconds(5);

            foreach (var force in m_additionalForces)
            {
                m_rigidBody.AddForce(force);
            }

            m_projNetwork = this.GetComponent<ProjectileSpawnManager>();

            m_deleteTimer = new Timer(TimeSpan.FromSeconds(m_deactivationTime));
            m_deleteTimer.OnTimerElapsed += DeleteTimer_OnTimerElapsed;
        }

        private void DeleteTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            m_projNetwork.DespawnProjectile(gameObject);
            m_isDespawning = true;
        }

        public void AddAdditionalForce(Vector3 force)
        {
            m_additionalForces.Add(force);
        }

        // Update is called once per frame
        public virtual void Update()
        {
            var currSpan = Time.time - m_startTime;

            if (m_lifeSpan < TimeSpan.FromSeconds(currSpan))
            {
                m_deleteTimer.Start();
                m_deleteTimer.Tick();

                DisableProjectile();
            }
        }

        private void DisableProjectile()
        {
            m_renderables.SetActive(false);
            m_collider.enabled = false;

            m_rigidBody.isKinematic = true;
            m_rigidBody.velocity = Vector3.zero;

            if (m_particleSystem != null)
            {
                var em = m_particleSystem.emission;
                em.enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            m_lifeSpan = TimeSpan.FromSeconds((Time.time - m_startTime));
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