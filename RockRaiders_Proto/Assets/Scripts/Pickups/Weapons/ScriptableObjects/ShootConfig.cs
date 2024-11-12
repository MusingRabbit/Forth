using System;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.ScriptableObjects
{
    [Serializable]
    //[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
    public class ShootConfig 
    {
        [SerializeField]
        private LayerMask m_hitMask;

        [SerializeField]
        private Vector3 m_spread;

        [SerializeField]
        private float m_fireRate;

        [SerializeField]
        private float m_range;

        [SerializeField]
        private int m_shotsPerRound;

        public LayerMask HitMask
        {
            get
            {
                return m_hitMask;
            }
        }

        public Vector3 Spread
        {
            get
            {
                return m_spread;
            }
        }

        public float FireRate
        {
            get
            {
                return m_fireRate;
            }
        }

        public float Range
        {
            get
            {
                return m_range;
            }
        }

        public int ShotsPerRound
        {
            get
            {
                return m_shotsPerRound;
            }
            set
            {
                m_shotsPerRound = value;
            }
        }

        public ShootConfig()
        {
            m_spread = Vector3.zero;
            m_fireRate = 0.25f;
            m_range = 50.0f;
            m_shotsPerRound = 1;
        }
    }
}
