using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Models
{
    public class SettingsModel
    {
        public GameSettingsModel GameSettings { get; set; }
        public MatchSettingsModel MatchSettings { get; set; }

        public SettingsModel()
        {
            this.GameSettings = new GameSettingsModel();
            this.MatchSettings = new MatchSettingsModel();
        }
    }
}
