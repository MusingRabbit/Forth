using Assets.Scripts.Services;

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
