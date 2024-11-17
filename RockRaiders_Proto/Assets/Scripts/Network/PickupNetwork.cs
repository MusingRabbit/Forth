using Assets.Scripts.Events;
using Assets.Scripts.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Assets.Scripts.Network
{
    internal class PickupNetwork : NetworkBehaviour
    {
        private NetworkObject m_netObj;
        private PickupItem m_pickupItem;
        private Timer m_despawnTimer;

        public PickupNetwork()
        {
            
        }

        private void Start()
        {
            m_pickupItem = this.GetComponent<PickupItem>();
            m_netObj = this.GetComponent<NetworkObject>();

            if (this.IsServer)
            {
                if (m_pickupItem.SelfDespawnEnabled)
                {
                    m_despawnTimer = new Timer(TimeSpan.FromSeconds(60));
                    m_despawnTimer.OnTimerElapsed += this.DespawnTimer_OnTimerElapsed;
                    m_despawnTimer.Start();
                }
            }
        }

        private void Update()
        {
            if (this.IsServer)
            {
                if (m_pickupItem.SelfDespawnEnabled && m_pickupItem.Owner == null)
                {
                    m_despawnTimer.Tick();
                }
            }
        }

        private void DespawnTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            if (this.IsServer)
            {
                m_netObj.Despawn(true);
            }
        }

    }
}
