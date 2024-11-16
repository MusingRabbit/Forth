using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Assets.Scripts.Network
{
    public class AudioManager : MonoBehaviour
    {
        private SettingsModel m_settings;

        [SerializeField]
        private Sound[] m_music;

        private List<Sound> m_gameSounds;

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
            }
        }

        public AudioManager()
        {
            m_gameSounds = new List<Sound>();
        }

        private void Awake()
        {
            foreach (var sound in m_music)
            {
                sound.Source = this.gameObject.AddComponent<AudioSource>();
                sound.Source.clip = sound.Clip;
                sound.Source.volume = sound.Volume * m_settings?.Game.MusicVolume ?? 1.0f;
                sound.Source.pitch = sound.Pitch;
                sound.Source.loop = sound.Loop;
            }

            this.PlayMusic("Theme");
        }

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

            foreach(var s in sounds)
            {
                m_gameSounds.Remove(s);
            }
        }

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

        private void UpdateVolume()
        {
            foreach (var sound in m_music)
            {
                sound.Source.volume = sound.Volume * m_settings.Game.MusicVolume;
            }

            foreach (var sound in m_gameSounds)
            {
                sound.Source.volume = sound.Volume * m_settings.Game.SoundVolume;
            }
        }

        public void AddGameSound(Sound sound)
        {
            m_gameSounds.Add(sound);
            sound.Source.volume = sound.Volume * m_settings.Game.SoundVolume;
        }

        private void GameSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.UpdateVolume();
        }
    }
}