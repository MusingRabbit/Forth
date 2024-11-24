using Assets.Scripts.Actor;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.Projectiles
{
    public class Explosive : MonoBehaviour
    {
        [SerializeField]
        private float m_explosionRadius;

        [SerializeField]
        private float m_explosionForce;

        [SerializeField]
        private GameObject m_particles;

        [SerializeField]
        private LayerMask m_layerMask;

        private Damage m_damage;

        public Damage Damage
        {
            get
            {
                return m_damage;
            }
            set
            {
                m_damage = value;
            }
        }

        public Explosive()
        {
            m_explosionRadius = 5.0f;
            m_explosionForce = 500.0f;
        }

        private void OnDestroy()
         {

            var proj = this.GetComponent<Projectile>();

            var surroundingObjs = Physics.OverlapSphere(this.transform.position, m_explosionRadius);
            foreach (var obj in surroundingObjs)
            {
                if (obj.GetComponent<ActorController>() != null)
                {
                    var rb = obj.GetComponent<Rigidbody>();
                    var hp = obj.GetComponent<Health>();
                    

                    if (rb != null)
                    {
                        rb.AddExplosionForce(m_explosionForce, this.transform.position, m_explosionRadius);
                    }

                    if (hp != null && m_damage != null)
                    {
                        var dVector = obj.transform.position - this.transform.position;
                        var distance = (dVector).magnitude;

                        if (Physics.Raycast(this.transform.position, dVector, out var hitInfo, distance))
                        {
                            var hitObj = hitInfo.collider.gameObject == obj.gameObject;

                            if (hitObj)
                            {
                                var ratio = 1.0f - (distance / m_explosionRadius);
                                m_damage.Multiplier = ratio;
                                hp.RegisterDamage(m_damage);

                                var actorState = obj.GetComponent<ActorState>();

                                if (actorState != null)
                                {
                                    var weapon = proj.Weapon;

                                    if (weapon != null)
                                    {
                                        actorState.LastHitBy = weapon.gameObject;
                                    }
                                }

                                NotificationService.Instance.NotifyPlayerAttacked(obj.gameObject);
                            }
                        }

                    }
                }
            }

            if (m_particles != null)
            {
                GameObject.Instantiate(m_particles, this.transform.position, Quaternion.identity);
            }
        }
    }
}
