using UnityEngine;
using System;
using Assets.Scripts.Events;
using Assets.Scripts.Services;
using Unity.VisualScripting;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Network;

namespace Assets.Scripts.Actor
{
    public class ActorState : RRMonoBehaviour
    {
        private bool m_stateChanged;
        public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

        private ActorHealth m_health;
        private ActorNetwork m_network;
        private Team m_team;
        private SelectedWeapon m_selectedWeapon;
        bool m_gravBootsEnabled;
        private bool m_isMoving;
        private bool m_isFloating;
        private bool m_feetOnGround;
        private bool m_isCrouched;
        private bool m_isMovingForward;
        private bool m_isDead;
        private int m_hp;
        private bool m_isDying;
        private string m_playerName;

        private GameObject m_lastHitBy;

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

        public SelectedWeapon SelectedWeapon => this.Inventory.SelectedWeapon;
        
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
        public bool IsDead
        {
            get
            {
                return m_isDead;
            }
        }

        public bool IsDying
        {
            get
            {
                return m_isDying;
            }
        }

        public int Health
        {
            get
            {
                return m_hp;
            }
        }

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

        public ActorInventory Inventory { get; set; }

        public ActorState()
        {

        }

        public override void Initialise()
        {
            this.Inventory = this.GetComponent<ActorInventory>();
            m_health = this.GetComponent<ActorHealth>();
            m_network = this.GetComponent<ActorNetwork>();
        }

        private void Start()
        {
            this.Initialise();
        }

        private void Update()
        {
            var isDying = m_health.State == ActorHealthState.Dying;
            var isDead = m_health.State == ActorHealthState.Dead;
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
                NotificationService.Instance.Info($"State Changed : {m_playerName} |Dying:{this.IsDying}|Dead:{this.IsDead}|HP:{hp}");
                 this.OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { Actor = this.gameObject, State = this  });
                m_stateChanged = false;
            }
        }

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
                    return null;
            }

            return null;
        }

        public void SelectWeapon(SelectedWeapon weapon)
        {
            this.Inventory.SelectWeapon(weapon);
            //if (this.SelectedWeapon == weapon)
            //{
            //    return;
            //}

            //switch (weapon)
            //{
            //    case SelectedWeapon.Main:
            //        if (this.Inventory.HasMainWeapon())
            //        {
            //            this.SelectedWeapon = SelectedWeapon.Main;
            //            this.Inventory.SelectWeapon(SelectedWeapon.Main);
            //        }
            //        break;
            //    case SelectedWeapon.Sidearm:
            //        if (this.Inventory.HasSideArm())
            //        {
            //            this.SelectedWeapon = SelectedWeapon.Sidearm;
            //            this.Inventory.SelectWeapon(SelectedWeapon.Sidearm);
            //        }
            //        break;
            //    case SelectedWeapon.None:
            //        this.SelectedWeapon = SelectedWeapon.None;
            //        this.Inventory.SelectWeapon(SelectedWeapon.None);
            //        break;
            //}
        }
    }
}
