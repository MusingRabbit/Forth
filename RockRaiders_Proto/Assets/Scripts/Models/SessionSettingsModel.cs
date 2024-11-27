using Assets.Scripts.Models;
using System;

namespace Assets.Scripts
{
    /// <summary>
    /// Match settings 
    /// </summary>
    public struct MatchSettings
    {
        /// <summary>
        /// Match score limit
        /// </summary>
        public int ScoreLimit;

        /// <summary>
        /// Match type, (Deathmatch, Team deathmatch, Capture the flag)
        /// </summary>
        public MatchType MatchType;

        /// <summary>
        /// Match time limit
        /// </summary>
        public TimeSpan TimeLimit;
    }

    /// <summary>
    /// Session settings
    /// Handles all of the settings configured for a particular session
    /// </summary>
    public class SessionSettingsModel : ObservableModel
    {
        private string m_localPlayerName;
        private string m_serverIp;
        private string m_level;
        private ushort m_port;
        private string m_serverName;
        private bool m_isHost;

        /// <summary>
        /// Gets or sets the player name for the local machine
        /// </summary>
        public string LocalPlayerName
        {
            get
            {
                return m_localPlayerName;
            }
            set
            {
                this.SetValue(ref m_localPlayerName, value);
            }
        }

        /// <summary>
        /// Gets or sets the server IP -> Connection IP of remote host
        /// </summary>
        public string ServerIP
        {
            get
            {
                return m_serverIp;
            }
            set
            {
                this.SetValue(ref m_serverIp, value);
            }
        }

        /// <summary>
        /// Gets or sets the server port -> Target port of remote host
        /// </summary>
        public ushort Port
        {
            get
            {
                return m_port;
            }
            set
            {
                this.SetValue(ref m_port, value);
            }
        }

        /// <summary>
        /// Gets or sets the server name - not currently in use
        /// </summary>
        public string ServerName
        {
            get
            {
                return m_serverName;
            }
            set
            {
                this.SetValue(ref m_serverName, value);
            }
        }

        /// <summary>
        /// Gets or sets whether is host
        /// </summary>
        public bool IsHost
        {
            get
            {
                return m_isHost;
            }
            set
            {
                this.SetValue(ref m_isHost, value);
            }
        }

        /// <summary>
        /// Gets or sets the current level
        /// </summary>
        public string Level
        {
            get
            {
                return m_level;
            }
            set
            {
                this.SetValue(ref m_level, value);
            }
        }

        /// <summary>
        /// Gets or sets the current match settings
        /// </summary>
        public MatchSettings MatchSettings { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SessionSettingsModel()
        {
            m_port = 7777;
            m_serverIp = "127.0.0.1";

            this.MatchSettings = new MatchSettings
            {
                MatchType = MatchType.Deathmatch,
                ScoreLimit = 15,
                TimeLimit = TimeSpan.Zero
            };

            m_serverName = "ServerName";
            m_localPlayerName = "PlayerName";
        }

        /// <summary>
        /// Gets the scene name for the selected match type and level
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Invalid match type</exception>
        public string GetSceneName()
        {
            switch (this.MatchSettings.MatchType)
            {
                case MatchType.Deathmatch:
                    return "DM-" + this.Level;
                case MatchType.TeamDeathmatch:
                    return "TDM-" + this.Level;
                case MatchType.CaptureTheFlag:
                    return "CTF-" + this.Level;
            }

            throw new InvalidOperationException();
        }
    }
}
