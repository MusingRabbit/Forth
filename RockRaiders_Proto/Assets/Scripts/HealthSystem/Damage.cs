using UnityEngine;

namespace Assets.Scripts.HealthSystem
{
    /// <summary>
    /// Damage component
    /// </summary>
    public class Damage : MonoBehaviour
    {
        /// <summary>
        /// Stores base damage
        /// </summary>
        [SerializeField]
        private int m_baseDamage;

        /// <summary>
        /// Stores any multiplier to be applied
        /// </summary>
        [SerializeField]
        private float m_multiplier;

        /// <summary>
        /// Gets or sets the base damage
        /// </summary>
        public int Base
        {
            get
            {
                return m_baseDamage;
            }
            set
            {
                m_baseDamage = value;
            }
        }

        /// <summary>
        /// Gets or sets the multiplier
        /// </summary>
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

        /// <summary>
        /// Gets the total damage
        /// </summary>
        public int Total
        {
            get
            {
                return (int)(this.Base * Multiplier);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Damage()
        {
            m_baseDamage = 1;
            m_multiplier = 1.0f;
        }
    }
}
