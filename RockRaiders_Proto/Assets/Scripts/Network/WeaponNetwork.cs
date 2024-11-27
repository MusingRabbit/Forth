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
    /// <summary>
    /// Weapon network data
    /// </summary>
    public struct WeaponStateData : INetworkSerializable
    {
        public int AmmoCount;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.AmmoCount);
        }
    }

    /// <summary>
    /// Weapon network behaviour
    /// </summary>
    public class WeaponNetwork : NetworkBehaviour
    {
        /// <summary>
        /// Weapon network data
        /// </summary>
        private NetworkVariable<WeaponStateData> m_wpnNetState;

        /// <summary>
        /// Weapon component 
        /// </summary>
        private Weapon m_weapon;

        /// <summary>
        /// Weapon update timer 
        /// </summary>
        private Timer m_weaponTimer;

        /// <summary>
        /// called on load
        /// </summary>
        private void Awake()
        {
            m_wpnNetState = new NetworkVariable<WeaponStateData>(writePerm: NetworkVariableWritePermission.Owner);
            m_weaponTimer = new Timer(TimeSpan.FromSeconds(1));
            m_weaponTimer.AutoReset = false;
        }

        /// <summary>
        /// called before first frame
        /// </summary>
        private void Start()
        {
            m_weapon = this.GetComponent<Weapon>();
            m_weaponTimer.Start();
        }

        /// <summary>
        /// called every frame
        /// -> If is server and update timer has lasped
        ///     -> notify all clients of weapons' ammo count 
        /// </summary>
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

        /// <summary>
        /// sends notification to all clients to update weapon ammo count
        /// </summary>
        /// <param name="data"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void TransmitWeaponStateClientRpc(WeaponStateData data)
        {
            m_weapon.Ammo = data.AmmoCount;
        }
    }
}
