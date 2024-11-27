using Assets.Scripts.Events;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Timer
    {
        /// <summary>
        /// Stores the time span of this timer
        /// </summary>
        private TimeSpan m_timerSpan = TimeSpan.MaxValue;

        /// <summary>
        /// Stores the number of seconds since timer was started / last reset 
        /// </summary>
        private float m_currSeconds;

        /// <summary>
        /// Stores whether the timer has been started
        /// </summary>
        private bool m_startTimer;

        /// <summary>
        /// Stores whether timer has lapsed
        /// </summary>
        private bool m_elapsed;

        /// <summary>
        /// Stores whether to automatically reset the timer when timer has elapsed
        /// </summary>
        private bool m_autoReset;

        /// <summary>
        /// Event fired whenever timer has elapsed
        /// </summary>
        public event EventHandler<TimerElapsedEventArgs> OnTimerElapsed;

        /// <summary>
        /// Returns whether timer has started
        /// </summary>
        public bool Started
        {
            get
            {
                return m_currSeconds > 0;
            }
        }

        /// <summary>
        /// Gets or sets whether the timer is to auto reset when lapsed
        /// </summary>
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

        /// <summary>
        /// Gets whether timer has elapsed
        /// </summary>
        public bool Elapsed
        {
            get
            {
                return m_elapsed;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Timer()
        {
            m_autoReset = true;
            m_elapsed = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeSpan">The amount of time the timer is to run before it is elapsed</param>
        public Timer(TimeSpan timeSpan)
            : this()
        {
            this.SetTimeSpan(timeSpan);
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Start()
        {
            m_startTimer = true;
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            m_startTimer = false;
        }

        /// <summary>
        /// Call every update (Unity)
        /// </summary>
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

        /// <summary>
        /// Sets the time span of this timer
        /// </summary>
        /// <param name="timeSpan">The amount of time to pass prior to the 'elapsed' flag being true</param>
        /// <param name="reset">Whether to reset the timer upon setting the timespan</param>
        public void SetTimeSpan(TimeSpan timeSpan, bool reset = true)
        {
            m_timerSpan = timeSpan;

            if (reset)
            {
                this.ResetTimer();
            }
        }

        /// <summary>
        /// Resets the current time (seconds) back to 0
        /// </summary>
        public void ResetTimer()
        {
            m_currSeconds = 0.0f;
            m_elapsed = false;
        }

    }
}
