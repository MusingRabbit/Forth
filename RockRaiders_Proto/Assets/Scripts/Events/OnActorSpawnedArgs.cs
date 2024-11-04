using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public class OnActorSpawnedArgs
    {
        GameObject m_actor;

        public GameObject Actor
        {
            get
            {
                return m_actor;
            }
        }

        public OnActorSpawnedArgs(GameObject actor)
        {
            m_actor = actor;
        }
    }
}
