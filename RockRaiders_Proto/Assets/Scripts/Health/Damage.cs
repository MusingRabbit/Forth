using UnityEngine;

namespace Assets.Scripts.Health
{
    public class Damage : MonoBehaviour
    {
        [SerializeField]
        private int m_baseDamage;

        [SerializeField]
        private float m_multiplier;

        public int Base
        {
            get
            {
                return m_baseDamage;
            }
        }

        public float Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        public Damage()
        {
            m_baseDamage = 1;
            m_multiplier = 1.0f;
        }
    }
}
