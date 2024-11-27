using Assets.Scripts.Models;

namespace Assets.Scripts
{
    public class GameSettingsModel : ObservableModel
    {
        private string m_playerName;
        private string m_resolution;
        private bool m_fullScreen;
        private float m_musicVol;
        private float m_soundVol;
        private float m_mouseSensitivity;

        /// <summary>
        /// Gets or sets the player name
        /// </summary>
        public string PlayerName
        {
            get
            {
                return m_playerName;
            }
            set
            {
                this.SetValue(ref m_playerName, value);
            }
        }

        /// <summary>
        /// Gets or sets the game resolution
        /// </summary>
        public string Resolution
        {
            get
            {
                return m_resolution;
            }
            set
            {
                this.SetValue(ref m_resolution, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the window is full screen or not
        /// </summary>
        public bool FullScreen
        {
            get
            {
                return m_fullScreen;
            }
            set
            {
                this.SetValue(ref m_fullScreen, value);
            }
        }

        /// <summary>
        /// Gets or sets the in game music volume
        /// </summary>
        public float MusicVolume
        {
            get
            {
                return m_musicVol;
            }
            set
            {
                this.SetValue(ref m_musicVol, value);
            }
        }

        /// <summary>
        /// Gets or sets the in game sound volume
        /// </summary>
        public float SoundVolume
        {
            get
            {
                return m_soundVol;
            }
            set
            {
                this.SetValue(ref m_soundVol, value);
            }
        }

        /// <summary>
        /// Gets or sets the games mouse sensetivity
        /// </summary>
        public float MouseSensetivity
        {
            get
            {
                return m_mouseSensitivity;
            }
            set
            {
                this.SetValue(ref m_mouseSensitivity, value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameSettingsModel()
        {
            m_resolution = "800x600@59.94";
            m_playerName = "Player";
            m_fullScreen = false;
            m_musicVol = 1.0f;
            m_soundVol = 1.0f;
            m_mouseSensitivity = 0.1f;
        }
    }
}
