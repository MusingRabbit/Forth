using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Util;
using System;
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

    public class Weapon : MonoBehaviour
    {
        public event EventHandler<OnShotFiredEventArgs> OnShotFired;


        [SerializeField]
        [SerializeAs("Name")]
        private string m_name;

        [SerializeField]
        [SerializeAs("WeaponSlot")]
        private WeaponSlot m_slot;

        [SerializeField]
        private int m_maxAmmo;

        private int m_ammoCount;

        private TriggerState m_triggerState;

        private GameObject m_owner;

        private Rigidbody m_rigidBody;
        private Rigidbody m_parentRigidBody;

        [SerializeField]
        private ActorCrosshair m_crosshair;

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
            m_name = "Weapon";
            m_maxAmmo = 150;
        }

        public virtual void Start()
        {
            m_rigidBody = this.GetComponent<Rigidbody>();
            this.ResetAmmo();
        }

        public virtual void Update()
        {
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
    }
}
