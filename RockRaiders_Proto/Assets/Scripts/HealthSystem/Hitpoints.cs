using Assets.Scripts.Services;
using System;

namespace Assets.Scripts.HealthSystem
{
    /// <summary>
    /// Used for storing (you guessed it) hit points. 
    /// </summary>
    public class Hitpoints
    {
        public event EventHandler<EventArgs> OnHitpointsDepleated;
        public event EventHandler<EventArgs> OnHitpointsFull;
        public event EventHandler<EventArgs> OnHitpointsRemoved;

        private int m_maxHp;

        private int m_currentHp;

        public int Max
        {
            get
            {
                return m_maxHp;
            }
        }

        public int Current
        {
            get
            {
                return m_currentHp;
            }
            set
            {
                m_currentHp = value;
            }
        }

        public Hitpoints()
        {
            m_maxHp = 100;
            m_currentHp = m_maxHp;
        }

        public void Start()
        {
            m_currentHp = m_maxHp;
        }

        public void Reset()
        {
            m_currentHp = m_maxHp;
        }

        public void SetHitPoints(int amount)
        {
            if (m_currentHp != amount)
            {
                m_currentHp = amount;

                if (m_currentHp <= 0)
                {
                    OnHitpointsDepleated?.Invoke(this, EventArgs.Empty);
                }

                if (m_currentHp >= m_maxHp)
                {
                    m_currentHp = m_maxHp;
                    OnHitpointsFull?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void AddHitPoints(int amount)
        {
            var newHp = m_currentHp + amount;
            var maxHp = m_maxHp <= newHp;

            newHp = !maxHp ? newHp : m_maxHp;

            if (newHp != m_currentHp)
            {
                m_currentHp = newHp;

                if (maxHp)
                {
                    OnHitpointsFull?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void RemoveHitPoints(int amount)
        {
            var newHp = m_currentHp - amount;
            var nilHp = newHp <= 0;

            newHp = !nilHp ? newHp : 0;

            if (newHp != m_currentHp)
            {
                m_currentHp = newHp;

                this.OnHitpointsRemoved?.Invoke(this, EventArgs.Empty);

                if (m_currentHp <= 0)
                {
                    OnHitpointsDepleated?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
