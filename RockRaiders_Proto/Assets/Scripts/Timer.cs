using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class Timer
    {
        private TimeSpan m_timerSpan = TimeSpan.MaxValue;
        private float m_currSeconds;
        private bool m_startTimer;
        private bool m_elapsed;

        public event EventHandler<TimerElapsedEventArgs> OnTimerElapsed;

        public Timer()
        {
            m_elapsed = false;
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
                OnTimerElapsed.Invoke(this, new TimerElapsedEventArgs());
                m_elapsed = true;
                this.ResetTimer();
                m_startTimer = false;
            }
        }

        public void SetTimeSpan(TimeSpan timeSpan)
        {
            m_timerSpan = timeSpan;
            this.ResetTimer();
        }

        public void ResetTimer()
        {
            m_currSeconds = 0.0f;
        }

    }
}
