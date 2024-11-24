using System;
using UnityEngine;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Services;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.Actor
{
    /// <summary>
    /// Behaviour component responsible for monitoring and modifying actors' health
    /// </summary>
    public class ActorHealth : HealthSystem.Health
    {
        /// <summary>
        /// Event that triggers whenever changes have been made to the actors' health.
        /// </summary>
        public event EventHandler<EventArgs> OnActorHealthStateChanged;

        /// <summary>
        /// Actor health status
        /// </summary>
        private ActorHealthStatus m_status;

        /// <summary>
        /// Actor state component
        /// </summary>
        private ActorState m_actorState;

        /// <summary>
        /// Actor audio component
        /// </summary>
        private ActorAudio m_actorAudio;

        /// <summary>
        /// Multiplier for headshots
        /// </summary>
        [SerializeField]
        private float m_headHitMultiplier;

        /// <summary>
        /// Multiplier for torso hits.
        /// </summary>
        [SerializeField]
        private float m_torsoHitMultiplier;

        /// <summary>
        /// Gets the actors health status.
        /// </summary>
        public ActorHealthStatus Status
        {
            get
            {
                return m_status;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorHealth()
        {
            
        }

        /// <summary>
        /// Start & Initialise - Called once before first frame in scene.
        /// </summary>
        protected override void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Update - Called every frame.
        /// </summary>
        private void Update()
        {
            if (m_status == ActorHealthStatus.Dying)                                // If actor status is dying or dead, handle death and return
            {       
                this.SetActorHealthStatus(ActorHealthStatus.Dead);                   
                return;
            }
            else if (m_status == ActorHealthStatus.Dead)                            

            {
                return;
            }

            m_status = this.Hitpoints.Current > 0 ? ActorHealthStatus.Live : ActorHealthStatus.Dying;  // Update actor status depening on available hitpoints
        }

        /// <summary>
        /// Resets actor health status
        /// </summary>
        public override void Reset()
        {
            m_status = ActorHealthStatus.Live;
            base.Reset();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_status = ActorHealthStatus.Live;
            base.Initialise();

            this.Hitpoints.OnHitpointsDepleated += this.Hitpoints_OnHitpointsDepleated;
            this.Hitpoints.OnHitpointsRemoved += this.Hitpoints_OnHitpointsRemoved;

            m_actorState = this.GetComponent<ActorState>();
            m_actorAudio = this.GetComponent<ActorAudio>();
        }

        /// <summary>
        /// Triggered when hitpoints are removed from actor health.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void Hitpoints_OnHitpointsRemoved(object sender, EventArgs e)
        {
            m_actorAudio.PlayRandomOuchSound();
        }

        /// <summary>
        /// Triggered when all hitpoints have been depleted from actor health.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void Hitpoints_OnHitpointsDepleated(object sender, EventArgs e)
        {
            this.SetActorHealthStatus(ActorHealthStatus.Dying);
        }

        /// <summary>
        /// Sets the actor health status
        /// </summary>
        /// <param name="status">Status</param>
        private void SetActorHealthStatus(ActorHealthStatus status)
        {
            if (m_status != status)
            {
                m_status = status;
                this.OnActorHealthStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Register a head hit.
        /// </summary>
        /// <param name="damage">Base Damage</param>
        public void RegisterHeadHit(Damage damage)
        {
            m_lastDamage = damage;
            this.Register(damage.Base, m_headHitMultiplier);
        }

        /// <summary>
        /// Register torso hit
        /// </summary>
        /// <param name="damage">Base Damage</param>
        public void RegisterTorsoHit(Damage damage)
        {
            m_lastDamage = damage;
            this.Register(damage.Base, m_torsoHitMultiplier);
        }

        /// <summary>
        /// Register projectile hit on torso
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterProjectileHitTorso(GameObject projectile)
        {
            this.RegisterProjectileHit(projectile, m_torsoHitMultiplier);
        }

        /// <summary>
        /// Register projectile hit on head
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterProjectileHitHead(GameObject projectile)
        {
            this.RegisterProjectileHit(projectile, m_headHitMultiplier);
        }

        /// <summary>
        /// Register projectile hit.
        /// </summary>
        /// <param name="projectile">Projectile</param>
        /// <param name="multiplier">Multiplier</param>
        public void RegisterProjectileHit(GameObject projectile, float multiplier)
        {
            var proj = projectile.GetComponent<Projectile>();
            var projRb = projectile.GetComponent<Rigidbody>();
            var projDmg = projectile.GetComponent<Damage>();

            if (proj.Weapon != null)
            {
                m_actorState.LastHitBy = proj.Weapon.gameObject;

                m_lastDamage = projDmg;
                this.Register(projDmg.Base, multiplier);

                NotificationService.Instance.NotifyPlayerAttacked(this.gameObject);
            }
        }
    }
}
