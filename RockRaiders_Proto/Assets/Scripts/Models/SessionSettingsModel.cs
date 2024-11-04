using System;

namespace Assets.Scripts
{
    public struct MatchSettings
    {
        public int ScoreLimit;
        public MatchType MatchType;
        public TimeSpan TimeLimit;
    }

    public class SessionSettingsModel
    {
        public string LocalPlayerName { get; set; }
        public string ServerIP { get; set; }
        public ushort Port { get; set; }
        public string ServerName { get; set; }
        public bool IsHost { get; set; }

        public MatchSettings MatchSettings { get; set; }
        public string Level { get; set; }

        public SessionSettingsModel()
        {
            this.Port = 7777;
            this.ServerIP = "127.0.0.1";

            this.MatchSettings = new MatchSettings
            {
                MatchType = MatchType.Deathmatch,
                ScoreLimit = 15,
                TimeLimit = TimeSpan.Zero
            };

            this.ServerName = "ServerName";
            this.LocalPlayerName = "PlayerName";
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
