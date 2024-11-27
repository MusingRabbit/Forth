using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    /// <summary>
    /// Base behaviour class for audio behaviour in rock raiders
    /// </summary>
    public class RRAudioBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Sound associated with this audio behaviour
        /// </summary>
        [SerializeField]
        private Sound[] m_sounds;

        /// <summary>
        /// The audio manager that manages this audio behaviour
        /// </summary>
        private AudioManager m_audioManager;

        /// <summary>
        /// Gets all sounds associated with this audio behaviour
        /// </summary>
        protected Sound[] Sounds
        {
            get
            {
                return m_sounds;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RRAudioBehaviour()
        {

        }

        /// <summary>
        /// Called when the behaviour is loaded
        /// </summary>
        protected virtual void Awake()
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
                //sound.Source.playOnAwake = false;

                m_audioManager.AddGameSound(sound);
            }
        }

        /// <summary>
        /// Play specified audio.
        /// </summary>
        /// <param name="name">Audio sound name</param>
        public void Play(string name)
        {
            for (int i = 0; i < m_sounds.Length; i++)
            {
                Sound sound = m_sounds[i];
                
                if (sound.Name == name && sound.Source != null)
                {
                    sound.Source.Play();
                    break;
                }
            }
        }

        /// <summary>
        /// Plays all sounds in sound list, if audio source is available.
        /// </summary>
        public void PlayAllSounds()
        {
            for (int i = 0; i < m_sounds.Length; i++)
            {
                Sound sound = m_sounds[i];

                if (sound.Source != null)
                {
                    sound.Source.Play();
                }
            }
        }

        /// <summary>
        /// Plays random sound in sound list, if audio source is available.
        /// </summary>
        /// <returns></returns>
        public bool PlayRandomSound()
        {
            if (this.Sounds.Length < 1)
            {
                return false;
            }

            var rndIdx = UnityEngine.Random.Range(0, this.Sounds.Length);
            var sound = this.Sounds[rndIdx];

            if (sound.Source != null)
            {
                sound.Source.Play();
            }

            return true;
        }
    }
}
