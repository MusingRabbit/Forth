using System;
using UnityEngine;
using Assets.Scripts.HealthSystem;

namespace Assets.Scripts.Actor
{
    public class ActorHealth : HealthSystem.Health
    {
        public event EventHandler<EventArgs> OnActorHealthStateChanged;

        private ActorHealthState m_state;

        [SerializeField]
        private float m_headHitMultiplier;

        //[SerializeField]
        private float m_torsoHitMultiplier;

        public ActorHealthState State
        {
            get
            {
                return m_state;
            }
        }

        public ActorHealth()
        {
            
        }

        protected override void Start()
        {
            this.Initialise();
        }

        private void Update()
        {
            if (m_state == ActorHealthState.Dying)
            {
                this.SetActorHealthState(ActorHealthState.Dead);
            }
        }

        public override void Reset()
        {
            m_state = ActorHealthState.Live;
            base.Reset();
        }

        public override void Initialise()
        {
            m_state = ActorHealthState.Live;
            base.Initialise();

            this.Hitpoints.OnHitpointsDepleated += this.Hitpoints_OnHitpointsDepleated;
        }

        private void Hitpoints_OnHitpointsDepleated(object sender, EventArgs e)
        {
            this.SetActorHealthState(ActorHealthState.Dying);
        }

        private void SetActorHealthState(ActorHealthState state)
        {
            if (m_state != state)
            {
                m_state = state;
                this.OnActorHealthStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RegisterHeadHit(Damage damage)
        {
            this.Register(damage.Base, m_headHitMultiplier);
        }

        public void RegisterTorsoHit(Damage damage)
        {
            this.Register(damage.Base, m_torsoHitMultiplier);
        }

        public void RegisterProjectileHitTorso(GameObject projectile)
        {
            this.RegisterProjectileHit(projectile, m_torsoHitMultiplier);
        }

        public void RegisterProjectileHitHead(GameObject projectile)
        {
            this.RegisterProjectileHit(projectile, m_headHitMultiplier);
        }

        public void RegisterProjectileHit(GameObject projectile, float multiplier)
        {
            var projRb = projectile.GetComponent<Rigidbody>();
            var projDmg = projectile.GetComponent<Damage>();

            var damage = this.CalculateDamageRigidBodyCollision(projRb, projDmg);
            this.Register(damage, multiplier);
        }
    }
}
