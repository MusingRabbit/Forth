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

    public class FlagBase : NetworkBehaviour
    {
        [SerializeField]
        private GameObject m_flagPrefab;

        [SerializeField]
        private Team m_team;

        [SerializeField]
        private Light m_light;

        private Flag m_flag;
        private FlagStand m_flagStand;
        private Rigidbody m_flagRb;
        private NetworkObject m_flagNetObj;
        private BoxCollider m_boxCol;

        private Timer m_resetTimer;
        private TimeSpan m_resetTimeSpan = TimeSpan.FromSeconds(30);

        public event EventHandler<EventArgs> FlagCaptured;

        private NetworkVariable<FlagBaseData> m_baseData;

        private bool FlagAtBase => m_boxCol.bounds.Contains(m_flag.transform.position);
        public Team Team
        {
            get
            {
                return m_team;
            }
        }

        public FlagBase()
        {
            m_flagStand = new FlagStand();
            m_baseData = new NetworkVariable<FlagBaseData>();

            m_resetTimer = new Timer(m_resetTimeSpan);
            m_resetTimer.OnTimerElapsed += ResetTimer_OnTimerElapsed;
        }

        // Start is called before the first frame update
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
        }

        // Update is called once per frame
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

            if (m_flag != null)
            {
                var fab = this.FlagAtBase;

                if (m_flag.Owner == null && !fab)
                {
                    m_resetTimer.Start();
                }

                if (fab && m_resetTimer.Elapsed)
                {
                    m_resetTimer.SetTimeSpan(m_resetTimeSpan);
                    m_resetTimer.ResetTimer();
                    m_resetTimer.Stop();
                }

                if (m_flagStand.Flag != null)
                {
                    m_flagRb.isKinematic = true;
                }
                else
                {
                    m_flagRb.isKinematic = false;
                }

                if (!this.FlagAtBase && m_flag.Retreived)
                {
                    if (this.IsServer)
                    {
                        m_resetTimer.SetTimeSpan(TimeSpan.Zero);
                    }
                }

                if (m_flag.Captured)
                {
                    if (m_flag.Owner != null)
                    {
                        var ownPickup = m_flag.Owner.GetComponent<ActorPickup>();
                        ownPickup.DropCurrentPack();
                    }

                    if (this.IsServer)
                    {
                        this.FlagCaptured?.Invoke(this, EventArgs.Empty);
                        m_resetTimer.SetTimeSpan(TimeSpan.Zero);
                    }
                }
            }

            m_resetTimer.Tick();
        }

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

        private Flag SpawnFlag()
        {
            var obj = GameObject.Instantiate(m_flagPrefab);

            var flag = obj.GetComponent<Flag>();
            var flagNet = obj.GetComponent<NetworkObject>();
            flag.Team = m_team;

            flagNet.Spawn(true);
            return flag;
        }

        private void ResetFlag()
        {
            m_flag.Initialise();

            var obj = m_flag.gameObject;
            var basePos = this.transform.position;
            obj.transform.position = new Vector3(basePos.x, basePos.y + obj.transform.localScale.y, basePos.z);
            obj.transform.rotation = this.transform.rotation;
            m_flag.Captured = false;
            m_flag.Retreived = false;
            m_flagStand.Flag = m_flag;
        }

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

        private void ResetTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            //m_resetTimer.SetTimeSpan(m_resetTimeSpan, false);
            m_resetTimer.Stop();

            if (this.IsServer)
            {
                var netTrans = m_flagNetObj.GetComponent<NetworkTransform>();
                //netTrans.Interpolate = false;
                this.ResetFlag();
                //netTrans.Interpolate = true;
            }
        }
    }
}