using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public class OnNotificationEventArgs
    {
        private MessageData m_data;

        public MessageData Data
        {
            get
            {
                return m_data;
            }
        }

        public OnNotificationEventArgs(MessageData data)
        {
            m_data = data;
        }
    }
}
