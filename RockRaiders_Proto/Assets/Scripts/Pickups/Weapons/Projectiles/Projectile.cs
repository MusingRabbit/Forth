using System;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.Projectiles
{
    public class Projectile : MonoBehaviour
    {


        [SerializeField]
        private TimeSpan m_lifeSpan;

        [SerializeField]
        private float m_mass;


        private float m_startTime;
        private float m_muzzleVelcity;

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

        private Rigidbody m_rigidBody;
        private ProjectileNetwork m_projNetwork;

        private void Awake()
        {

        }


        // Start is called before the first frame update
        public virtual void Start()
        {
            m_rigidBody = GetComponent<Rigidbody>();
            m_rigidBody.velocity += transform.forward * m_muzzleVelcity;
            m_rigidBody.mass = m_mass;
            m_startTime = Time.time;
            m_lifeSpan = TimeSpan.FromSeconds(5);

            m_projNetwork = GetComponent<ProjectileNetwork>();
        }

        // Update is called once per frame
        public virtual void Update()
        {
            var currSpan = Time.time - m_startTime;

            if (m_lifeSpan < TimeSpan.FromSeconds(currSpan))
            {
                //Destroy(this.gameObject);
                m_projNetwork.DespawnProjectile(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            //Destroy(this.gameObject);
            m_projNetwork.DespawnProjectile(gameObject);
            //Destroy(this.gameObject);
        }
    }
}