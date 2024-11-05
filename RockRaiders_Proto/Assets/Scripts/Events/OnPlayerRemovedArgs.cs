﻿using Assets.Scripts.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public class OnPlayerRemovedArgs : EventArgs
    {
        private PlayerMatchData m_playerData;

        public PlayerMatchData PlayerData
        {
            get
            {
                return m_playerData;
            }
        }

        public OnPlayerRemovedArgs(PlayerMatchData playerData)
        {
            m_playerData = playerData;
        }
    }
}