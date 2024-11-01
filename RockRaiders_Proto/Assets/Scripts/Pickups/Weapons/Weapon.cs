using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons
{
    public enum WeaponSlot
    {
        Main,
        Sidearm,
        Melee
    }

    public enum TriggerState
    {
        Released,
        Pulled
    }

    public class Weapon : PickupItem
    {
        public event EventHandler<OnShotFiredEventArgs> OnShotFired;


        [SerializeField]
        [SerializeAs("Name")]
        private string m_name;

        [SerializeField]
        [SerializeAs("WeaponSlot")]
        private WeaponSlot m_slot;

        [SerializeField]
        private int m_weaponDamage;

        [SerializeField]
        private WeaponType m_type;

        [SerializeField]
        private int m_maxAmmo;

        private int m_ammoCount;

        private TriggerState m_triggerState;

        private GameObject m_owner;

        private Timer m_dropTimer;

        private Rigidbody m_rigidBody;
        private Rigidbody m_parentRigidBody;

        [SerializeField]
        private ActorCrosshair m_crosshair;

        private ulong m_networkOwnerId;

        public bool CanFire => m_ammoCount >= 0 || m_maxAmmo == -1;

        public int MaxAmmo
        {
            get
            {
                return m_maxAmmo;
            }
        }

        public int Ammo
        {
            get
            {
                return m_ammoCount;
            }
        }

        public int Damage
        {
            get
            {
                return m_weaponDamage;
            }
        }

        public GameObject Owner
        {
            get
            {
                return m_owner;
            }
            set
            {
                m_owner = value;
                m_parentRigidBody = m_owner?.GetComponent<Rigidbody>();
                m_networkOwnerId = m_owner?.GetComponent<NetworkObject>().NetworkObjectId ?? uint.MaxValue;
            }
        }

        public ulong NetworkObjectId
        {
            get
            {
                return m_networkOwnerId;
            }
        }

        public ActorCrosshair Crosshair
        {
            get
            {
                return m_crosshair;
            }
            set
            {
                m_crosshair = value;
            }
        }

        protected Rigidbody OwnerRigidBody
        {
            get
            {
                return m_parentRigidBody;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            protected set
            {
                m_name = value;
            }
        }

        public WeaponSlot WeaponSlot
        {
            get
            {
                return m_slot;
            }
            protected set
            {
                m_slot = value;
            }
        }

        public WeaponType WeaponType
        {
            get
            {
                return m_type;
            }
        }

        public TriggerState TriggerState
        {
            get
            {
                return m_triggerState;
            }
            set
            {
                m_triggerState = value;
            }
        }

        public Weapon()
        {
            m_dropTimer = new Timer(TimeSpan.FromSeconds(3));
            m_dropTimer.AutoReset = false;
            m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;

            m_slot = WeaponSlot.Main;
            m_triggerState = TriggerState.Released;
            m_name = "Weapon";
            m_maxAmmo = 150;
        }

        protected override void Start()
        {
            base.Start();

            m_rigidBody = this.GetComponent<Rigidbody>();
            this.ResetAmmo();

            if (m_type == WeaponType.None)
            {
                NotificationService.Instance.Warning("Weapon type is set to 'None'! " + this.Name);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();
                m_rigidBody.includeLayers = LayerMask.GetMask("Level", "Default");
                m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
            }

            switch (m_triggerState)
            {
                case TriggerState.Released:
                    break;
                case TriggerState.Pulled:
                    this.Fire();
                    break;
            }

            m_triggerState = TriggerState.Released;
            m_dropTimer.Tick();

            if (m_dropTimer.Elapsed)
            {
                m_dropTimer.Stop();
                //m_rigidBody.detectCollisions = true;
            }
        }

        public virtual void Fire()
        {

        }

        protected void Invoke_OnShotFired(Vector3 velocity, float mass)
        {
            this.DecreaseAmmoCount();
            this.OnShotFired?.Invoke(this, new OnShotFiredEventArgs(velocity, mass));
        }

        private void DecreaseAmmoCount()
        {
            m_ammoCount--;
        }

        public void Reset()
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();       // For some reason instantiated objects dont always run Start().....
            }

            m_rigidBody.ResetVelocity();
            this.ResetAmmo();
        }

        public void ResetAmmo()
        {
            m_ammoCount = m_maxAmmo;
        }

        private void DropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_rigidBody.includeLayers = LayerMask.GetMask("Level", "Default");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
        }

        public void SetPickedUp()
        {
            m_rigidBody.velocity = Vector3.zero;
            m_rigidBody.angularVelocity = Vector3.zero;
            m_rigidBody.detectCollisions = false;
            m_rigidBody.includeLayers = LayerMask.GetMask("Level");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Default");
        }

        public void SetDropped()
        {
            m_rigidBody.detectCollisions = true;
            m_rigidBody.excludeLayers = LayerMask.GetMask("Default");
            m_rigidBody.includeLayers = LayerMask.GetMask("Level");
            m_dropTimer.ResetTimer();
            m_dropTimer.Start();
        }
    }
}
