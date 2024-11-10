using Assets.Scripts.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    internal class OnFlagCapturedEventArgs
    {
        private GameObject m_actor;
        private Team m_team;

        public Team CapturingTeam
        {
            get
            {
                return m_team;
            }
        }

        public GameObject Player
        {
            get
            {
                return m_actor;
            }
        }

        public OnFlagCapturedEventArgs(GameObject actor)
        {
            m_actor = actor;
            m_team = actor.GetComponent<ActorState>().Team;
        }
    }
}
