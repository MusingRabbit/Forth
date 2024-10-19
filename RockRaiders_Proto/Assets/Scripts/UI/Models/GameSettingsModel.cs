using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameSettingsModel
    {
        public string PlayerName { get; set; }
        public string Resolution { get; set; }
        public bool FullScreen { get; set; }

        public GameSettingsModel()
        {
            this.Resolution = "800x600@59.94";
        }
    }
}
