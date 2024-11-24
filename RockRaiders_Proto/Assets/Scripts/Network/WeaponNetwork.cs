using Assets.Scripts.Pickups.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;

namespace Assets.Scripts.Network
{
    public struct WeaponStateData : INetworkSerializable
    {
        public int AmmoCount;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.AmmoCount);
        }
    }

    public class WeaponNetwork : NetworkBehaviour
    {
        private NetworkVariable<WeaponStateData> m_wpnNetState;
        private Weapon m_weapon;
        private Timer m_weaponTimer;

        private void Awake()
        {
            m_wpnNetState = new NetworkVariable<WeaponStateData>(writePerm: NetworkVariableWritePermission.Owner);
            m_weaponTimer = new Timer(TimeSpan.FromSeconds(1));
            m_weaponTimer.AutoReset = false;
        }

        private void Start()
        {
            m_weapon = this.GetComponent<Weapon>();
            m_weaponTimer.Start();
        }

        private void Update()
        {
            m_weaponTimer.Tick();

            if (m_weaponTimer.Elapsed)
            {
                m_weaponTimer.ResetTimer();

                if (this.IsServer)
                {
                    this.TransmitWeaponStateClientRpc(new WeaponStateData { AmmoCount = m_weapon.Ammo });
                }
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void TransmitWeaponStateClientRpc(WeaponStateData data)
        {
            m_weapon.Ammo = data.AmmoCount;
        }
    }
}
