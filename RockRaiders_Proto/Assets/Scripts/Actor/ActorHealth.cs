using System;
using UnityEngine;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Services;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.Actor
{
    public class ActorHealth : HealthSystem.Health
    {
        public event EventHandler<EventArgs> OnActorHealthStateChanged;

        private ActorHealthState m_state;
        private ActorState m_actorState;
        private ActorAudio m_actorAudio;

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
                return;
            }
            else if (m_state == ActorHealthState.Dead)
            {
                return;
            }

            m_state = this.Hitpoints.Current > 0 ? ActorHealthState.Live : ActorHealthState.Dying;
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
            this.Hitpoints.OnHitpointsRemoved += this.Hitpoints_OnHitpointsRemoved;

            m_actorState = this.GetComponent<ActorState>();
            m_actorAudio = this.GetComponent<ActorAudio>();
        }

        private void Hitpoints_OnHitpointsRemoved(object sender, EventArgs e)
        {
            m_actorAudio.PlayRandomOuchSound();
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
            m_lastDamage = damage;
            this.Register(damage.Base, m_headHitMultiplier);
        }

        public void RegisterTorsoHit(Damage damage)
        {
            m_lastDamage = damage;
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
            var proj = projectile.GetComponent<Projectile>();
            var projRb = projectile.GetComponent<Rigidbody>();
            var projDmg = projectile.GetComponent<Damage>();

            m_actorState.LastHitBy = proj.Weapon.gameObject;

            m_lastDamage = projDmg;
            this.Register(projDmg.Base, multiplier);

            NotificationService.Instance.NotifyPlayerAttacked(this.gameObject, new Damage { Base = projDmg.Base, Multiplier = multiplier });
        }
    }
}
