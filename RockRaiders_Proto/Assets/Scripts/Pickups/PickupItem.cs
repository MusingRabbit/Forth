using Assets.Scripts.Events;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using System.Xml.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Pickups
{
    public class PickupItem : RRMonoBehaviour
    {
        [SerializeField]
        [SerializeAs("Name")]
        private string m_name;

        protected PackType m_packType;

        private Timer m_dropTimer;

        protected Rigidbody m_rigidBody;
        private Rigidbody m_parentRigidBody;
        private ulong m_networkOwnerId;

        [SerializeField]
        private bool m_selfDespawn;

        private GameObject m_owner;
        private BoxCollider m_trigger;

        public PackType PackType
        {
            get
            {
                return m_packType;
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

        protected Rigidbody OwnerRigidBody
        {
            get
            {
                return m_parentRigidBody;
            }
        }

        public bool SelfDespawnEnabled
        {
            get
            {
                return m_selfDespawn;
            }
            set
            {
                m_selfDespawn = value;
            }
        }

        public event EventHandler<EventArgs> OnPickedUp;
        public event EventHandler<EventArgs> OnDropped;

        public PickupItem()
        {
            m_dropTimer = new Timer(TimeSpan.FromSeconds(3));
            m_dropTimer.AutoReset = false;
            m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
        }

        public override void Initialise()
        {
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_trigger = this.GetComponent<BoxCollider>();

            m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            this.Initialise();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();
            }

            if (this.Owner == null && m_dropTimer.Elapsed == false)
            {
                m_dropTimer.Start();
            }

            m_dropTimer.Tick();

            if (m_dropTimer.Elapsed)
            {
                m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
                m_dropTimer.Stop();
            }
        }

        public override void Reset()
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();       // For some reason instantiated objects dont always run Start().....
            }

            m_rigidBody.ResetVelocity();
        }

        public void SetPickedUp(bool suppressNotifications = false)
        {
            m_rigidBody.velocity = Vector3.zero;
            m_rigidBody.angularVelocity = Vector3.zero;
            //m_rigidBody.isKinematic = true;
            m_rigidBody.constraints = RigidbodyConstraints.None;

            //m_rigidBody.includeLayers = LayerMask.GetMask("Nothing");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Players");
            m_trigger.includeLayers = m_rigidBody.includeLayers;
            m_trigger.excludeLayers = m_rigidBody.excludeLayers;

            m_dropTimer.Stop();
            m_dropTimer.ResetTimer();

            if (!suppressNotifications)
            {
                this.OnPickedUp?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetDropped(bool suppressNotifications = false)
        {
            //m_rigidBody.excludeLayers = LayerMask.GetMask("Default");
            //m_rigidBody.includeLayers = LayerMask.GetMask("Nothing");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Players");
            m_trigger.includeLayers = m_rigidBody.includeLayers;
            m_trigger.excludeLayers = m_rigidBody.excludeLayers;
            //m_rigidBody.isKinematic = false;
            m_dropTimer.ResetTimer();
            m_dropTimer.Start();

            if (!suppressNotifications)
            {
                this.OnDropped?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
            m_trigger.excludeLayers = m_rigidBody.excludeLayers;
            m_dropTimer.ResetTimer();
            m_dropTimer.Stop();
        }


    }
}