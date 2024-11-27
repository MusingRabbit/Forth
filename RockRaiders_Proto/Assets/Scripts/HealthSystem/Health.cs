using System;
using UnityEngine;

namespace Assets.Scripts.HealthSystem
{
    /// <summary>
    /// Health
    /// </summary>
    public class Health : RRMonoBehaviour, IHealth
    {
        /// <summary>
        /// Stores reference to hitpoints component
        /// </summary>
        private Hitpoints m_hp;

        /// <summary>
        /// Stores reference to rigid body component
        /// </summary>
        private Rigidbody m_rigidBody;

        /// <summary>
        /// Stores last damage component
        /// </summary>
        protected Damage m_lastDamage;


        /// <summary>
        /// Gets hitpoints
        /// </summary>
        public Hitpoints Hitpoints
        {
            get
            {
                return m_hp;
            }
        }

        /// <summary>
        /// Gets the last damage component to have intereacted with this health component
        /// </summary>
        public Damage LastDamage
        {
            get
            {
                return m_lastDamage;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Health()
        {
            m_hp = new Hitpoints();
        }

        /// <summary>
        /// Called prior to the first frame in scene.
        /// </summary>
        protected virtual void Start()
        {
            Initialise();
        }

        /// <summary>
        /// Resets this component
        /// </summary>
        public override void Reset()
        {
            m_hp.Reset();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_rigidBody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Registers damage. Takes the damage, and applies to to this health component.
        /// </summary>
        /// <param name="damage">Damage to be registered</param>
        public void RegisterDamage(Damage damage)
        {
            m_lastDamage = damage;
            Register(damage.Base, damage.Multiplier);
        }

        /// <summary>
        /// Takes damage and multiplier and removes the calculated number of hitpoints from the hitpoints
        /// </summary>
        /// <param name="damage">Base damage</param>
        /// <param name="multiplier">Multiplier</param>
        protected void Register(int damage, float multiplier)
        {
            m_hp.RemoveHitPoints((int)MathF.Round(multiplier * damage));
        }

        /// <summary>
        /// Registers a rigid body collision
        /// </summary>
        /// <param name="rigidBody">Object that has been hit</param>
        /// <exception cref="NullReferenceException">rigidbody param cannot be null</exception>
        /// <exception cref="InvalidOperationException">This gameobject has no rigid body - calculation cannot continue</exception>
        public void RegisterRigidBodyCollision(Rigidbody rigidBody)
        {
            if (rigidBody == null)
            {
                throw new NullReferenceException(nameof(rigidBody));
            }

            if (m_rigidBody == null)
            {
                throw new InvalidOperationException("GameObject has no rigidbody.");
            }

            int damage;

            if (rigidBody != null)
            {
                damage = (int)(Mathf.Abs(m_rigidBody.mass - rigidBody.mass) * (m_rigidBody.velocity + rigidBody.velocity).magnitude);
            }
            else
            {
                damage = (int)(rigidBody.mass * rigidBody.velocity.magnitude);
            }

            Register(damage, 1.0f);
        }

        /// <summary>
        /// Calculates the damage caused by a rigid body collision
        /// </summary>
        /// <param name="rhsRigidBody">RigidBody component attatched to other entity</param>
        /// <param name="rhsDamage">Damage component attatched to other entity</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">rhsRigidBody param cannot be null</exception>
        /// <exception cref="InvalidOperationException">This gameobject has no rigid body - calculation cannot continue</exception>
        protected int CalculateDamageRigidBodyCollision(Rigidbody rhsRigidBody, Damage rhsDamage)
        {
            if (rhsRigidBody == null)
            {
                throw new NullReferenceException(nameof(rhsRigidBody));
            }

            if (m_rigidBody == null)
            {
                throw new InvalidOperationException("GameObject has no rigidbody.");
            }

            m_lastDamage = rhsDamage;
            var rhsObj = rhsDamage.gameObject;
            var damage = rhsDamage.Base;

            return rhsDamage.Base * (int)(Math.Abs(m_rigidBody.mass - rhsRigidBody.mass) * (m_rigidBody.velocity + rhsRigidBody.velocity).magnitude);
        }

        /// <summary>
        /// Registers a rigidbody collision and associated damage component
        /// </summary>
        /// <param name="rhsRigidBody">RigidBody on other entity</param>
        /// <param name="rhsDamage">Damage on other entity</param>
        public void RegisterRigidBodyCollisionWithDamage(Rigidbody rhsRigidBody, Damage rhsDamage)
        {
            Register(CalculateDamageRigidBodyCollision(rhsRigidBody, rhsDamage), rhsDamage.Multiplier);
        }
    }
}
