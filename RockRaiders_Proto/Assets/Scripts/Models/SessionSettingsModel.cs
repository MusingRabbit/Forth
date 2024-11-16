﻿using Assets.Scripts.Models;
using System;

namespace Assets.Scripts
{
    public struct MatchSettings
    {
        public int ScoreLimit;
        public MatchType MatchType;
        public TimeSpan TimeLimit;
    }

    public class SessionSettingsModel : ObservableModel
    {
        private string m_localPlayerName;
        private string m_serverIp;
        private string m_level;
        private ushort m_port;
        private string m_serverName;
        private bool m_isHost;

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

        public MatchSettings MatchSettings { get; set; }

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
