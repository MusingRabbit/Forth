using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Assets.Scripts
{
    /// <summary>
    /// Audio manager
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// Stores a reference to the settings model
        /// </summary>
        private SettingsModel m_settings;

        /// <summary>
        /// Keeps a list of all music sounds to be played by this audio manager
        /// </summary>
        [SerializeField]
        private Sound[] m_music;

        /// <summary>
        /// Keeps a list of all game sounds to be played by this audio manager.
        /// </summary>
        private List<Sound> m_gameSounds;

        /// <summary>
        /// Gets or sets the settings. 
        /// On set -> Applies settings
        /// </summary>
        public SettingsModel Settings
        {
            get
            {
                return m_settings;
            }
            set
            {
                if (m_settings != null)
                {
                    m_settings.Game.PropertyChanged -= this.GameSettings_PropertyChanged;
                }

                m_settings = value;
                m_settings.Game.PropertyChanged += this.GameSettings_PropertyChanged;
                this.UpdateVolume();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AudioManager()
        {
            m_gameSounds = new List<Sound>();
        }

        /// <summary>
        /// Called on load
        /// </summary>
        private void Awake()
        {
            foreach (var sound in m_music)
            {
                sound.Source = this.gameObject.AddComponent<AudioSource>();
                sound.Source.clip = sound.Clip;

                if (m_settings != null)
                {
                    sound.Source.volume = sound.Volume * m_settings.Game.MusicVolume;
                }
                else
                {
                    sound.Source.volume = 1.0f;
                }
                
                sound.Source.pitch = sound.Pitch;
                sound.Source.loop = sound.Loop;
            }

            this.PlayMusic("Theme");
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            var sounds = new List<Sound>();

            for (int i = 0; i < m_gameSounds.Count; i++)
            {
                Sound sound = m_gameSounds[i];
                if (sound.Source == null)
                {
                    sounds.Add(sound);
                }
            }

            foreach (var s in sounds)
            {
                m_gameSounds.Remove(s);
            }
        }

        /// <summary>
        /// Search for and play the music specified (if found)
        /// </summary>
        /// <param name="name">The name of the track to be played.</param>
        /// <returns>Track found & play started? (true / false)</returns>
        public bool PlayMusic(string name)
        {
            var result = false;

            foreach (var sound in m_music)
            {
                if (sound.Source.isPlaying)
                {
                    sound.Source.Stop();
                }

                if (sound.Name == name)
                {
                    if (!sound.Source.isPlaying)
                    {
                        sound.Source.Play();
                        result = true;
                    }
                }
            }

            if (!result)
            {
                NotificationService.Instance.Warning($"Could not find sound '{name}'");
            }

            return result;
        }


        /// <summary>
        /// Search for and stop the music specified (if found)
        /// </summary>
        /// <param name="name">The name of the track to be played.</param>
        /// <returns>Track found & play stopped? (true / false)</returns>
        public bool Stop(string name)
        {
            var result = false;

            foreach (var sound in m_music)
            {
                if (sound.Name == name)
                {
                    sound.Source.Stop();
                    result = true;
                }
            }

            if (!result)
            {
                NotificationService.Instance.Warning($"Could not find sound '{name}'");
            }

            return result;
        }

        /// <summary>
        /// Updates the volume of all game and music sounds to match that of the settings 
        /// </summary>
        private void UpdateVolume()
        {
            foreach (var sound in m_music)
            {
                if (sound.Source != null)
                {
                    sound.Source.volume = sound.Volume * m_settings.Game.MusicVolume;
                }
            }

            foreach (var sound in m_gameSounds)
            {
                if (sound.Source != null)
                {
                    sound.Source.volume = sound.Volume * m_settings.Game.SoundVolume;
                }
            }
        }

        /// <summary>
        /// Adds a game sound to be managed by this audio manager
        /// </summary>
        /// <param name="sound">Sound to be managed</param>
        public void AddGameSound(Sound sound)
        {
            m_gameSounds.Add(sound);
            sound.Source.volume = sound.Volume * m_settings.Game.SoundVolume;
        }

        /// <summary>
        /// Called whenever a game setting property has been changed.
        /// -> Updates volume
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Property event args</param>
        private void GameSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.UpdateVolume();
        }
    }
}