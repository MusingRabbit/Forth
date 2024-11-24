using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using System.Diagnostics;
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
        private WeaponSlot m_slot;

        [SerializeField]
        private int m_weaponDamage;

        [SerializeField]
        private WeaponType m_type;

        [SerializeField]
        private int m_maxAmmo;
        
        [SerializeField]
        private ActorCrosshair m_crosshair;

        private int m_ammoCount;

        private TriggerState m_triggerState;


        private WeaponAudio m_weaponAudio;

        public bool CanFire => m_ammoCount > 0 || m_maxAmmo == -1;

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
            set
            {
                m_ammoCount = value;
            }
        }

        public int Damage
        {
            get
            {
                return m_weaponDamage;
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
            m_slot = WeaponSlot.Main;
            m_triggerState = TriggerState.Released;
            this.Name = "Weapon";
            m_maxAmmo = 150;

            
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Initialise()
        {
            this.ResetAmmo();

            if (m_type == WeaponType.None)
            {
                NotificationService.Instance.Warning("Weapon type is set to 'None'! " + this.Name);
            }

            m_weaponAudio = this.GetComponent<WeaponAudio>();

            base.Initialise();
        }

        protected override void Update()
        {
            base.Update();

            switch (m_triggerState)
            {
                case TriggerState.Released:
                    break;
                case TriggerState.Pulled:
                    this.Fire();
                    break;
            }

            m_triggerState = TriggerState.Released;
        }

        public virtual void Fire()
        {
            if (m_weaponAudio != null)
            {
                m_weaponAudio.PlayRandomSound();
            }

            this.DecreaseAmmoCount();
        }

        protected void Invoke_OnShotFired(Vector3 velocity, float mass)
        {
            this.OnShotFired?.Invoke(this, new OnShotFiredEventArgs(velocity, mass));
        }

        protected void DecreaseAmmoCount()
        {
            if (m_ammoCount > 0)
            {
                m_ammoCount--;
            }
        }

        public override void Reset()
        {
            this.ResetAmmo();
            base.Reset();
        }

        public void ResetAmmo()
        {
            m_ammoCount = m_maxAmmo;
        }


        public void ResetRigidBody()
        {
            m_rigidBody.constraints = RigidbodyConstraints.FreezePosition;
            m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
        }

        private void OnTriggerEnter(Collider other)
        {
            var actorPickup = other.gameObject.GetComponent<ActorPickup>();

            if (actorPickup != null)
            {
                actorPickup.PickupWeapon(this, false);
            }
        }
    }
}
