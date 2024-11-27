using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Assets.Scripts.Level
{
    public struct FlagStand
    {
        public Flag Flag;
    }

    public struct FlagBaseData : INetworkSerializable
    {
        public ulong FlagNetworkObjectId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.FlagNetworkObjectId);
        }
    }

    /// <summary>
    /// Flag base
    /// </summary>
    public class FlagBase : NetworkBehaviour
    {
        /// <summary>
        /// Flag prefab
        /// </summary>
        [SerializeField]
        private GameObject m_flagPrefab;

        /// <summary>
        /// Flag base team
        /// </summary>
        [SerializeField]
        private Team m_team;

        /// <summary>
        /// Flag base light component
        /// </summary>
        [SerializeField]
        private Light m_light;

        /// <summary>
        /// Stores the flag component of the flag instance 
        /// </summary>
        private Flag m_flag;

        /// <summary>
        /// Stores the flag stand
        /// </summary>
        private FlagStand m_flagStand;

        /// <summary>
        /// Stores the flags' rigidbody component
        /// </summary>
        private Rigidbody m_flagRb;

        /// <summary>
        /// Stores the flags' network object
        /// </summary>
        private NetworkObject m_flagNetObj;

        /// <summary>
        /// Stores the box collider of the flag base
        /// </summary>
        private BoxCollider m_boxCol;

        /// <summary>
        /// Flag reset timer
        /// </summary>
        private Timer m_resetTimer;

        /// <summary>
        /// Flag reset timespan
        /// </summary>
        private TimeSpan m_resetTimeSpan = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Flag captured event
        /// </summary>
        public event EventHandler<EventArgs> FlagCaptured;

        /// <summary>
        /// Flag base data
        /// </summary>
        private NetworkVariable<FlagBaseData> m_baseData;

        /// <summary>
        /// Gets whether the flag is back at base.
        /// </summary>
        private bool FlagAtBase => m_boxCol.bounds.Contains(m_flag.transform.position);
        
        /// <summary>
        /// Gets the team of the flag base.
        /// </summary>
        public Team Team
        {
            get
            {
                return m_team;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FlagBase()
        {
            m_flagStand = new FlagStand();
            m_baseData = new NetworkVariable<FlagBaseData>();

            m_resetTimer = new Timer(m_resetTimeSpan);
            m_resetTimer.OnTimerElapsed += ResetTimer_OnTimerElapsed;
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            m_boxCol = this.GetComponent<BoxCollider>();

            if (this.IsServer)
            {
                m_flag = this.SpawnFlag();

                m_flagNetObj = m_flag.GetComponent<NetworkObject>();

                m_flagRb = m_flag.GetComponent<Rigidbody>();

                m_baseData.Value = new FlagBaseData { FlagNetworkObjectId = m_flagNetObj.NetworkObjectId };

                m_flagStand.Flag = m_flag;

                this.ResetFlag();
            }

            var debugMesh = this.gameObject.FindChild("DebugMesh");

            if (debugMesh != null)
            {
                debugMesh.SetActive(false);
            }
        }

        /// <summary>
        /// Called every frame
        ///     -> If is client 
        ///         -> Get flag game object if no reference exists
        ///     -> If is server
        ///         -> Start reset timer if flag is picked up and not at base
        ///         -> If flag is at base
        ///             -> Resets the reset timer and flag stand
        ///             -> Constrains flag movement
        ///         -> Else
        ///             -> If retreived 
        ///                 -> Resets flag
        ///             -> If Captured
        ///                 -> Resets flag
        ///                 -> Fires flag captured event
        /// </summary>
        void Update()
        {
            if (this.IsClient)
            {
                if (m_flag == null)
                {
                    m_flag = GetFlagByNetworkObjectId(m_baseData.Value.FlagNetworkObjectId);
                    m_flagRb = m_flag.GetComponent<Rigidbody>();
                    m_flagNetObj = m_flag.GetComponent<NetworkObject>();
                }
            }

            if (this.IsServer)
            {
                if (m_flag != null)
                {
                    if (m_flag.Owner == null && !this.FlagAtBase)
                    {
                        m_resetTimer.Start();
                    }

                    if (this.FlagAtBase)
                    {
                        m_resetTimer.SetTimeSpan(m_resetTimeSpan);
                        m_resetTimer.ResetTimer();
                        m_resetTimer.Stop();

                        m_flagStand.Flag = m_flag;
                    }

                    if (m_flagStand.Flag != null)
                    {
                        //m_flagRb.isKinematic = true;
                        m_flagRb.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        //m_flagRb.isKinematic = false;
                        m_flagRb.constraints = RigidbodyConstraints.None;
                    }

                    if (!this.FlagAtBase && m_flag.Retreived)
                    {
                        m_resetTimer.SetTimeSpan(TimeSpan.Zero);
                    }

                    if (m_flag.Captured)
                    {
                        if (m_flag.Owner != null)
                        {
                            var ownPickup = m_flag.Owner.GetComponent<ActorPickup>();
                            ownPickup.DropCurrentPack();
                        }

                        this.FlagCaptured?.Invoke(this, EventArgs.Empty);
                        m_resetTimer.SetTimeSpan(TimeSpan.Zero);
                        m_flag.Captured = false;
                    }
                }

                m_resetTimer.Tick();
            }
        }

        /// <summary>
        /// Gets the flag by network object id
        /// </summary>
        /// <param name="networkObjId">network object id</param>
        /// <returns>flag</returns>
        private Flag GetFlagByNetworkObjectId(ulong networkObjId)
        {
            Flag result = null;
            var spawnedObjs = NetworkManager.SpawnManager.SpawnedObjects;

            if (spawnedObjs.ContainsKey(networkObjId))
            {
                result = spawnedObjs[networkObjId].GetComponent<Flag>();
            }
            
            return result;
        }

        /// <summary>
        /// Spawns flag for this base
        /// </summary>
        /// <returns>flag</returns>
        private Flag SpawnFlag()
        {
            var obj = GameObject.Instantiate(m_flagPrefab);

            var flag = obj.GetComponent<Flag>();
            var flagNet = obj.GetComponent<NetworkObject>();
            flag.Team = m_team;

            flagNet.Spawn(true);
            return flag;
        }

        /// <summary>
        /// Resets flag to this base
        /// </summary>
        private void ResetFlag()
        {
            m_flag.Initialise();

            var obj = m_flag.gameObject;
            var basePos = this.transform.position;
            obj.transform.position = new Vector3(basePos.x, basePos.y + obj.transform.localScale.y, basePos.z);
            obj.transform.rotation = this.transform.rotation;
            m_flagRb.constraints = RigidbodyConstraints.FreezeAll;
            m_flag.Captured = false;
            m_flag.Retreived = false;
            m_flagStand.Flag = m_flag;
        }

        /// <summary>
        /// Called when object has entered the box collider of this base
        ///     -> Checks to see whether the colliding object is a flag
        ///     -> If the flag belongs to the other team, and own flag is at base
        ///         -> Mark flag as having been captured
        /// </summary>
        /// <param name="other">Other collider</param>
        private void OnTriggerEnter(Collider other)
        {
            NotificationService.Instance.Info(other.name);

            var flag = other.gameObject.GetComponent<Flag>();
            var isFlag = flag != null;

            if (this.FlagAtBase && isFlag)
            {
                if (flag.Team != m_team)
                {
                    flag.Captured = true;
                }
            }
        }

        /// <summary>
        /// Called when reset timer lapses
        /// </summary>
        /// <param name="sender">Sender -> Reset timer</param>
        /// <param name="e">Event args <see cref="Events.TimerElapsedEventArgs"/></param>
        private void ResetTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            m_resetTimer.Stop();

            if (this.IsServer)
            {
                var netTrans = m_flagNetObj.GetComponent<NetworkTransform>();
                this.ResetFlag();
            }
        }
    }
}