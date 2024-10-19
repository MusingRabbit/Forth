using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class MatchSettingsModel
    {
        public string LocalPlayerName { get; set; }
        public string ServerIP { get; set; }
        public ushort Port { get; set; }
        public string ServerName { get; set; }
        public MatchType MatchType { get; set; }
        public string Level { get; set; }
        public bool IsHost { get; set; }

        public MatchSettingsModel()
        {
            this.Port = 7777;
            this.ServerIP = "127.0.0.1";
            this.MatchType = MatchType.Deathmatch;
            this.ServerName = "ServerName";
            this.LocalPlayerName = "PlayerName";
        }

        public string GetSceneName()
        {
            switch (this.MatchType)
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
