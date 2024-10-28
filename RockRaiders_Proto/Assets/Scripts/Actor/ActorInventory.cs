using Assets.Scripts.Pickups.Weapons;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts
{
    public struct NetActorInventory : INetworkSerializable, IEquatable<NetActorInventory>
    {
        public WeaponType MainWeaponType;
        public ulong MainWeaponNetworkObjectId;
        public WeaponType SideArmType;
        public ulong SidearmNetworkObjectId;
        public WeaponType Pack;

        public bool Equals(NetActorInventory other)
        {
            return 
                this.MainWeaponNetworkObjectId == other.MainWeaponNetworkObjectId &&
                this.SidearmNetworkObjectId == other.SidearmNetworkObjectId &&
                this.MainWeaponType == other.MainWeaponType && 
                this.SideArmType == other.SideArmType && 
                this.Pack == other.Pack;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.MainWeaponType);
            serializer.SerializeValue(ref this.SideArmType);
        }
    }

    public class ActorInventoryState
    {
        public WeaponType MainWeaponType { get; set; }
        public Weapon MainWeapon { get; set; }
        public WeaponType SideArmType { get; set; }
        public Weapon SideArm { get; set; }
        public WeaponType PackType { get; set; }
    }

    public class ActorInventory : RRMonoBehaviour
    {
        [SerializeField]
        private GameObject m_mainWeapon;

        [SerializeField]
        private GameObject m_mainWeaponHolster;

        [SerializeField]
        private GameObject m_sideArm;

        [SerializeField]
        private GameObject m_sideArmHolster;

        [SerializeField]
        private GameObject m_pack;

        [SerializeField]
        private GameObject m_packHolster;

        [SerializeField]
        private SelectedWeapon m_selectedWeapon;

        public SelectedWeapon SelectedWeapon
        {
            get
            {
                return m_selectedWeapon;
            }
            set
            {
                m_selectedWeapon = value;
            }
        }

        public ActorInventory()
        {
            m_selectedWeapon = SelectedWeapon.None;
        }

        public override void Initialise()
        {
            m_selectedWeapon = SelectedWeapon.None;
        }

        private void LateUpdate()
        {
            this.UpdateStoredItemWorldPos();
        }

        private void Update()
        {
            
        }

        public override void Reset()
        {
            this.ClearMainWeapon();
            this.ClearSideArm();
            m_selectedWeapon = SelectedWeapon.None;
        }

        private void UpdateStoredItemWorldPos()
        {
            var sideArmObj = this.GetSideArm();

            if (m_selectedWeapon != SelectedWeapon.Sidearm && sideArmObj != null)
            {
                sideArmObj.transform.position = m_sideArmHolster.transform.position;
                sideArmObj.transform.rotation = m_sideArmHolster.transform.rotation;
            }

            var mainWeaponObj = this.GetMainWeapon();

            if (m_selectedWeapon != SelectedWeapon.Main && mainWeaponObj != null)
            {
                mainWeaponObj.transform.position = m_mainWeaponHolster.transform.position;
                mainWeaponObj.transform.rotation = m_mainWeaponHolster.transform.rotation;
            }
        }

        public bool HasMainWeapon()
        {
            return m_mainWeapon != null;
        }

        public void SetMainWeapon(GameObject mainWeapon)
        {
            if (m_mainWeapon == null)
            {
                m_mainWeapon = mainWeapon;
                this.ConfigureRigidBodyOnPickup(mainWeapon);
            }

        }

        public NetActorInventory GetNetActorInventory()
        {
            var weaponObj = this.GetMainWeapon();
            var mwNetworkObjId = weaponObj.GetComponent<NetworkObject>().NetworkObjectId;

            var sidearmObj = this.GetSideArm();
            var saNetworkObjId = weaponObj.GetComponent<NetworkObject>().NetworkObjectId;

            return new NetActorInventory
            {
                MainWeaponType = GetWeaponType(SelectedWeapon.Main),
                MainWeaponNetworkObjectId = mwNetworkObjId,
                SideArmType = GetWeaponType(SelectedWeapon.Sidearm),
                SidearmNetworkObjectId = saNetworkObjId
            };
        }

        public void SetInventoryFromActorInventoryState(ActorInventoryState state)
        {
            if (this.HasMainWeapon())
            {
                if (state.MainWeaponType != this.GetMainWeaponType())
                {
                    this.ClearMainWeapon();
                }
            }
            else if (state.MainWeapon != null)
            {
                this.SetMainWeapon(state.MainWeapon.gameObject);
            }

            if (this.HasSideArm())
            {
                if (state.SideArmType != this.GetSideArmType())
                {
                    this.ClearSideArm();
                }
            }
            else if (state.SideArm != null)
            {
                this.SetMainWeapon(state.SideArm.gameObject);
            }




        }

        private void ConfigureRigidBodyOnPickup(GameObject weapon)
        {
            weapon.GetComponent<Weapon>().SetPickedUp();
            weapon.GetComponent<Rigidbody>().detectCollisions = false;
        }

        public WeaponType GetMainWeaponType()
        {
            return this.GetMainWeapon().GetComponent<Weapon>().WeaponType;
        }

        public WeaponType GetSideArmType()
        {
            return this.GetSideArm().GetComponent<Weapon>().WeaponType;
        }

        private WeaponType GetWeaponType(SelectedWeapon selectedWeapon)
        {
            switch (selectedWeapon)
            {
                case SelectedWeapon.None:
                    return WeaponType.None;
                case SelectedWeapon.Main:
                    return this.GetMainWeapon().GetComponent<Weapon>().WeaponType;
                case SelectedWeapon.Sidearm:
                    return this.GetSideArm().GetComponent<Weapon>().WeaponType;
                case SelectedWeapon.Pack:
                    break;
            }

            return WeaponType.None;
        }

        private void ConfigureRigidBodyOnDrop(GameObject weapon)
        {
            weapon.GetComponent<Weapon>().SetDropped();
            var rb = weapon.GetComponent<Rigidbody>();
            rb.detectCollisions = true;
        }

        public GameObject GetMainWeapon()
        {
            return m_mainWeapon;
        }

        public bool HasSideArm()
        {
            return m_sideArm != null;
        }

        public void SetSideArm(GameObject sideArm)
        {
            if (m_sideArm == null)
            {
                m_sideArm = sideArm;
                this.ConfigureRigidBodyOnPickup(sideArm);
            }
        }

        public GameObject GetSideArm()
        {
            return m_sideArm;
        }

        public void ClearMainWeapon()
        {
            if (m_mainWeapon != null)
            {
                this.ConfigureRigidBodyOnDrop(m_mainWeapon);

                m_mainWeapon = null;

                if (m_selectedWeapon == SelectedWeapon.Main)
                {
                    this.SelectWeapon(SelectedWeapon.None);
                }
            }
        }

        public void ClearSideArm()
        {
            if (m_sideArm != null)
            {
                this.ConfigureRigidBodyOnDrop(m_sideArm);

                m_sideArm = null;

                if (m_selectedWeapon == SelectedWeapon.Sidearm)
                {
                    this.SelectWeapon(SelectedWeapon.None);
                }
            }
        }

        public void SelectWeapon(SelectedWeapon selection)
        {
            m_selectedWeapon = SelectedWeapon.None;

            switch (selection)
            {
                case SelectedWeapon.Main:

                    if (m_mainWeapon != null)
                    {
                        m_selectedWeapon = selection;
                    }
                    break;
                case SelectedWeapon.Sidearm:

                    if (m_sideArm != null)
                    {
                        m_selectedWeapon = selection;
                    }
                    break;
            }
        }

        public GameObject GetSelectedWeapon()
        {
            switch (m_selectedWeapon)
            {
                case SelectedWeapon.Main:
                    return m_mainWeapon;
                case SelectedWeapon.Sidearm:
                    return m_sideArm;
                case SelectedWeapon.Pack:
                    break;
            };

            return null;
        }
    }
}
