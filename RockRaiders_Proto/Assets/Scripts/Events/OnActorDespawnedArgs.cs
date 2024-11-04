using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public class OnActorDespawnedArgs
    {
        private ulong m_clientId;

        public ulong ClientId
        {
            get
            {
                return m_clientId;
            }
        }

        public OnActorDespawnedArgs(ulong clientId)
        {
            m_clientId = clientId;
        }
    }
}
