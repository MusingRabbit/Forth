using System;
using UnityEngine;

namespace Assets.Scripts.HealthSystem
{
    public class Health : RRMonoBehaviour
    {
        private Hitpoints m_hp;
        private Rigidbody m_rigidBody;

        public Hitpoints Hitpoints
        {
            get
            {
                return m_hp;
            }
        }

        public Health()
        {
            m_hp = new Hitpoints();
        }

        protected virtual void Start()
        {
            Initialise();
        }

        public override void Reset()
        {
            m_hp.Reset();
        }

        public override void Initialise()
        {
            m_rigidBody = GetComponent<Rigidbody>();
        }

        public void RegisterDamage(Damage damage)
        {
            Register(damage.Base, damage.Multiplier);
        }

        protected void Register(int damage, float multiplier)
        {
            m_hp.RemoveHitPoints((int)MathF.Round(multiplier * damage));
        }

        public void RegisterRigidBodyCollision(Rigidbody rigidBody)
        {
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

        protected int CalculateDamageRigidBodyCollision(Rigidbody rhsRigidBody, Damage rhsDamage)
        {
            if (m_rigidBody == null)
            {
                throw new InvalidOperationException("GameObject has no rigidbody.");
            }

            var rhsObj = rhsDamage.gameObject;
            var damage = rhsDamage.Base;

            return rhsDamage.Base * (int)(Math.Abs(m_rigidBody.mass - rhsRigidBody.mass) * (m_rigidBody.velocity + rhsRigidBody.velocity).magnitude);
        }

        public void RegisterRigidBodyCollisionWithDamage(Rigidbody rhsRigidBody, Damage rhsDamage)
        {
            Register(CalculateDamageRigidBodyCollision(rhsRigidBody, rhsDamage), rhsDamage.Multiplier);
        }
    }
}
