using Assets.Scripts.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public class OnPickupEventArgs : EventArgs
    {
        private PickupItem m_item;

        public PickupItem Item
        {
            get
            {
                return m_item;
            }
        }

        public OnPickupEventArgs(PickupItem pickupItem)
        {
            m_item = pickupItem;
        }
    }
}
