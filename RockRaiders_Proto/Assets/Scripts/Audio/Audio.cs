using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    public class RRAudioBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Sound[] m_sounds;

        private AudioManager m_audioManager;

        protected Sound[] Sounds
        {
            get
            {
                return m_sounds;
            }
        }

        public RRAudioBehaviour()
        {

        }

        private void Awake()
        {
            m_audioManager = FindObjectOfType<AudioManager>();

            for (int i = 0; i < m_sounds.Length; i++)
            {
                Sound sound = m_sounds[i];
                sound.Source = gameObject.AddComponent<AudioSource>();
                sound.Source.clip = sound.Clip;
                sound.Source.volume = sound.Volume;
                sound.Source.pitch = sound.Pitch;
                sound.Source.loop = sound.Loop;
                sound.Source.spatialBlend = 1.0f;

                m_audioManager.AddGameSound(sound);

                sound.Source.Play();
            }
        }
    }
}
