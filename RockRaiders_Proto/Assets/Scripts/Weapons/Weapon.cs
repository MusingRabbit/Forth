using Assets.Scripts.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Weapons
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
        [SerializeField]
        [SerializeAs("Name")]
        private string m_name;

        [SerializeField]
        [SerializeAs("WeaponSlot")]
        private WeaponSlot m_slot;

        private TriggerState m_triggerState;

        private GameObject m_owner;

        private Rigidbody m_parentRigidBody;

        [SerializeField]
        private ActorCrosshair m_crosshair;

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
        }

        public virtual void Start()
        {

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
    }
}
