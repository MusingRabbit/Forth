using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts
{
    [Serializable]
    public class Sound 
    {
        [SerializeField]
        private AudioClip m_clip;

        [SerializeField]
        private string m_name;

        [SerializeField]
        [Range(0f, 1.0f)]
        private float m_volume;

        [SerializeField]
        [Range(0.1f, 3.0f)]
        private float m_pitch;

        [SerializeField]
        private bool m_loop;

        private AudioSource m_source;

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public AudioSource Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
            }
        }

        public AudioClip Clip
        {
            get
            {
                return m_clip;
            }
            set
            {
                m_clip = value;
            }
        }

        public float Volume
        {
            get
            {
                return m_volume;
            }
            set
            {
                m_volume = value;
            }
        }

        public float Pitch
        {
            get
            {
                return m_pitch;
            }
            set
            {
                m_pitch = value;
            }
        }

        public bool Loop
        {
            get
            {
                return m_loop;
            }
            set
            {
                m_loop = value;
            }
        }

        public Sound()
        {
            m_volume = 0.8f;
            m_pitch = 1.0f;
        }
    }
}
