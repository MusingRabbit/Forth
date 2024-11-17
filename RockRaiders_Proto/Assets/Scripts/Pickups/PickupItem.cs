using Assets.Scripts.Events;
using Assets.Scripts.Util;
using System;
using System.Xml.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Pickups
{
    public class PickupItem : MonoBehaviour
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

        public PickupItem()
        {
            m_dropTimer = new Timer(TimeSpan.FromSeconds(3));
            m_dropTimer.AutoReset = false;
            m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            m_rigidBody = this.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();
                m_rigidBody.includeLayers = LayerMask.GetMask("Level", "Default");
                m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
            }

            m_dropTimer.Tick();

            if (m_dropTimer.Elapsed)
            {
                m_dropTimer.Stop();
            }
        }

        private void Reset()
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = this.GetComponent<Rigidbody>();       // For some reason instantiated objects dont always run Start().....
            }

            m_rigidBody.ResetVelocity();
        }

        public void SetPickedUp()
        {
            m_rigidBody.velocity = Vector3.zero;
            m_rigidBody.angularVelocity = Vector3.zero;
            m_rigidBody.isKinematic = true;
            m_rigidBody.includeLayers = LayerMask.GetMask("Level");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Default");
        }

        public void SetDropped()
        {
            m_rigidBody.excludeLayers = LayerMask.GetMask("Default");
            m_rigidBody.includeLayers = LayerMask.GetMask("Level");
            m_rigidBody.isKinematic = false;
            m_dropTimer.ResetTimer();
            m_dropTimer.Start();
        }

        private void DropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_rigidBody.includeLayers = LayerMask.GetMask("Level", "Default");
            m_rigidBody.excludeLayers = LayerMask.GetMask("Nothing");
            m_dropTimer.ResetTimer();
            m_dropTimer.Stop();
        }
    }
}