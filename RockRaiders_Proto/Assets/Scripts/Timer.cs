using Assets.Scripts.Events;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Timer
    {
        private TimeSpan m_timerSpan = TimeSpan.MaxValue;
        private float m_currSeconds;
        private bool m_startTimer;
        private bool m_elapsed;

        private bool m_autoReset;

        public event EventHandler<TimerElapsedEventArgs> OnTimerElapsed;

        public bool Started
        {
            get
            {
                return m_currSeconds > 0;
            }
        }

        public bool AutoReset
        {
            get
            {
                return m_autoReset;
            }
            set
            {
                m_autoReset = value;
            }
        }

        public bool Elapsed
        {
            get
            {
                return m_elapsed;
            }
        }

        public Timer()
        {
            m_autoReset = true;
            m_elapsed = false;
        }

        public Timer(TimeSpan timeSpan)
            : this()
        {
            this.SetTimeSpan(timeSpan);
        }

        public void Start()
        {
            m_startTimer = true;

        }

        public void Stop()
        {
            m_startTimer = false;
        }

        public void Tick()
        {
            if (m_startTimer)
            {
                m_currSeconds += Time.deltaTime;
            }

            if (TimeSpan.FromSeconds(m_currSeconds) > m_timerSpan)
            {
                OnTimerElapsed?.Invoke(this, new TimerElapsedEventArgs());
                m_elapsed = true;

                if (m_autoReset)
                {
                    this.ResetTimer();
                }
            }
        }

        public void SetTimeSpan(TimeSpan timeSpan, bool reset = true)
        {
            m_timerSpan = timeSpan;

            if (reset)
            {
                this.ResetTimer();
            }
        }

        public void ResetTimer()
        {
            m_currSeconds = 0.0f;
            m_elapsed = false;
        }

    }
}
