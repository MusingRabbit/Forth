using UnityEngine;
using System;
using Assets.Scripts.Events;
using Assets.Scripts.Services;
using Unity.VisualScripting;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Network;

namespace Assets.Scripts.Actor
{
    /// <summary>
    /// Actor state
    /// </summary>
    public class ActorState : RRMonoBehaviour
    {
        /// <summary>
        /// Flag indicating whether this state has been changed since last update
        /// </summary>
        private bool m_stateChanged;

        /// <summary>
        /// Fired whenever a state change has been detected
        /// </summary>
        public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// Stores a reference to the actors' health component
        /// </summary>
        private ActorHealth m_health;

        /// <summary>
        /// Stores the actors' team
        /// </summary>
        private Team m_team;
        
        /// <summary>
        /// Stores the actors' currently selected weapon
        /// </summary>
        private SelectedWeapon m_selectedWeapon;

        /// <summary>
        /// Stores whether grav boots have been enabled or not
        /// </summary>
        bool m_gravBootsEnabled;

        /// <summary>
        /// Stores whether actor is moving
        /// </summary>
        private bool m_isMoving;

        /// <summary>
        /// Stores whether actor is floating
        /// </summary>
        private bool m_isFloating;

        /// <summary>
        /// Stores whether actor has feet on the ground
        /// </summary>
        private bool m_feetOnGround;

        /// <summary>
        /// Stores whether actor is currently crouched
        /// </summary>
        private bool m_isCrouched;

        /// <summary>
        /// Stores whether actor is moving forward
        /// </summary>
        private bool m_isMovingForward;

        /// <summary>
        /// Sores whether actor is dead
        /// </summary>
        private bool m_isDead;

        /// <summary>
        /// Stores actor hitpoints that it retreives from Actors' health component
        /// </summary>
        private int m_hp;

        /// <summary>
        /// Stores whether actor is in dying state (just been killed)
        /// </summary>
        private bool m_isDying;

        /// <summary>
        /// Stores the actors' player name
        /// </summary>
        private string m_playerName;

        /// <summary>
        /// Stores the game object that last hit this actor
        /// </summary>
        private GameObject m_lastHitBy;

        /// <summary>
        /// Gets or sets the actor's team
        /// </summary>
        public Team Team
        {
            get
            {
                return m_team;
            }
            set
            {
                if (m_team != value)
                {
                    m_team = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gameobject that last hit the actor
        /// </summary>
        public GameObject LastHitBy
        {
            get
            {
                return m_lastHitBy;
            }
            set
            {
                m_lastHitBy = value;
            }
        }

        /// <summary>
        /// Gets the currently selected weapon
        /// </summary>
        public SelectedWeapon SelectedWeapon => this.Inventory.SelectedWeapon;
        
        /// <summary>
        /// Gets or sets whether grav boots have been enabled.
        /// </summary>
        public bool GravBootsEnabled
        {
            get
            {
                return m_gravBootsEnabled;
            }
            set
            {
                if (m_gravBootsEnabled != value)
                {
                    m_gravBootsEnabled = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actor is moving
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return m_isMoving;
            }
            set
            {
                if (m_isMoving != value)
                {
                    m_isMoving = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actor is floating
        /// </summary>
        public bool IsFloating
        {
            get
            {
                return m_isFloating;
            }
            set
            {
                if (m_isFloating != value)
                {
                    m_isFloating = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actors' feet are grounded.
        /// </summary>
        public bool FeetOnGround
        {
            get
            {
                return m_feetOnGround;
            }
            set
            {
                if (m_feetOnGround != value)
                {
                    m_feetOnGround = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actor is crouched
        /// </summary>
        public bool IsCrouched
        {
            get
            {
                return m_isCrouched;
            }
            set
            {
                if (m_isCrouched != value)
                {
                    m_isCrouched = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actor is moving forward.
        /// </summary>
        public bool IsMovingForward
        {
            get
            {
                return m_isMovingForward;
            }
            set
            {
                if (m_isMovingForward != value)
                {
                    m_isMovingForward = value;
                    m_stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether actor is dead.
        /// </summary>
        public bool IsDead
        {
            get
            {
                return m_isDead;
            }
            set
            {
                m_isDead = value;
            }
        }

        /// <summary>
        /// Gets or sets whether actor is dying.
        /// </summary>
        public bool IsDying
        {
            get
            {
                return m_isDying;
            }
            set
            {
                m_isDying = value;
            }
        }

        /// <summary>
        /// Gets actor health
        /// </summary>
        public int Health
        {
            get
            {
                return m_hp;
            }
        }

        /// <summary>
        /// Gets actors' player name
        /// </summary>
        public string PlayerName
        {
            get
            {
                return m_playerName;
            }
            set
            {
                m_playerName = value;
            }
        }

        /// <summary>
        /// Gets actors' inventory
        /// </summary>
        public ActorInventory Inventory { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorState()
        {

        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            this.Inventory = this.GetComponent<ActorInventory>();
            m_health = this.GetComponent<ActorHealth>();
        }

        /// <summary>
        /// Called before first frame in scene
        /// </summary>
        private void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Called every frame
        ///     -> Checks for any state changes and invokes on state changed event if so.
        /// </summary>
        private void Update()
        {
            var isDying = m_health.Status == ActorHealthStatus.Dying;
            var isDead = m_health.Status == ActorHealthStatus.Dead;
            var hp = m_health?.Hitpoints.Current ?? 0;

            if (isDying != m_isDying)
            {
                m_isDying = isDying;
                m_stateChanged = true;
            }

            if (isDead != m_isDead)
            {
                m_isDead = isDead;
                m_isDying = false;
                m_stateChanged = true;
            }

            if (hp != m_hp)
            {
                m_hp = hp;
                m_stateChanged = true;
            }

            if (m_stateChanged)
            {
                 this.OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { Actor = this.gameObject, State = this  });
                m_stateChanged = false;
            }
        }

        /// <summary>
        /// Resets all fields and flags belonging to this component
        /// </summary>
        public override void Reset()
        {

            this.GravBootsEnabled = true;
            this.IsFloating = false;
            this.IsMoving = false;
            this.IsCrouched = false;
            this.FeetOnGround = false;
            this.IsMovingForward = false;
            this.Team = Team.None;
            m_isDead = false;
            m_isDying = false;
        }

        /// <summary>
        /// Gets the currently selected weapon for this actor.
        /// </summary>
        /// <returns>Weapon game object</returns>
        public GameObject GetSelectedWeapon()
        {
            switch (this.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return null;
                case SelectedWeapon.Main:
                    return this.Inventory.GetMainWeapon();
                case SelectedWeapon.Sidearm:
                    return this.Inventory.GetSideArm();
                case SelectedWeapon.Pack:
                    return this.Inventory.GetPackItem();
            }

            return null;
        }

        /// <summary>
        /// Selects weapon
        /// </summary>
        /// <param name="weapon">Weapon to be selected</param>
        public void SelectWeapon(SelectedWeapon weapon)
        {
            this.Inventory.SelectWeapon(weapon);
        }
    }
}
