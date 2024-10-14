using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
    public class ShootConfigurationScriptableObject : ScriptableObject
    {
        [SerializeField]
        private LayerMask m_hitMask;

        [SerializeField]
        private Vector3 m_spread = new Vector3(0.0f, 0.0f, 0.1f);

        [SerializeField]
        private float m_fireRate = 0.25f;

        [SerializeField]
        private float m_range = 50.0f;

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
    }
}
