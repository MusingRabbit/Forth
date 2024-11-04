using Assets.Scripts.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public class OnPlayerSwitchTeamsArgs
    {
        private PlayerMatchData m_playerData;
        private Team m_team;


        public PlayerMatchData PlayerData
        {
            get
            {
                return m_playerData;
            }
        }

        public Team Team
        {
            get
            {
                return m_team;
            }
        }

        public OnPlayerSwitchTeamsArgs(PlayerMatchData playerData, Team team)
        {
            m_playerData = playerData;
            m_team = team;
        }
    }
}
