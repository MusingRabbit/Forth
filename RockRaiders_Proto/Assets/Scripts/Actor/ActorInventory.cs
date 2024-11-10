using Assets.Scripts.Events;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
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
        public PackType PackType;
        public ulong PackNetworkObjectId;

        public bool Equals(NetActorInventory other)
        {
            return 
                this.MainWeaponNetworkObjectId == other.MainWeaponNetworkObjectId &&
                this.SidearmNetworkObjectId == other.SidearmNetworkObjectId &&
                this.MainWeaponType == other.MainWeaponType && 
                this.SideArmType == other.SideArmType && 
                this.PackType == other.PackType;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.MainWeaponType);
            serializer.SerializeValue(ref this.MainWeaponNetworkObjectId);
            serializer.SerializeValue(ref this.SideArmType);
            serializer.SerializeValue(ref this.SidearmNetworkObjectId);
            serializer.SerializeValue(ref this.PackType);
            serializer.SerializeValue(ref this.PackNetworkObjectId);
        }
    }

    public class ActorInventoryState : IEquatable<ActorInventoryState>
    {
        public WeaponType MainWeaponType { get; set; }
        public Weapon MainWeapon { get; set; }
        public WeaponType SideArmType { get; set; }
        public Weapon SideArm { get; set; }
        public PackType PackType { get; set; }
        public PickupItem Pack { get; set; }

        public bool Equals(ActorInventoryState other)
        {
            return this.MainWeaponType == other.MainWeaponType &&
                this.SideArmType == other.SideArmType &&
                this.MainWeapon == other.MainWeapon &&
                this.SideArm == other.SideArm &&
                this.PackType == other.PackType && 
                this.Pack == other.Pack;
        }
    }

    public class ActorInventory : RRMonoBehaviour
    {
        public event EventHandler<OnPickupEventArgs> OnMainWeaponCleared;
        public event EventHandler<OnPickupEventArgs> OnSidearmCleared;
        public event EventHandler<OnPickupEventArgs> OnPackCleared;

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
            if (this.HasSideArm())
            {
                var sideArmObj = this.GetSideArm();

                if (m_selectedWeapon != SelectedWeapon.Sidearm && sideArmObj != null)
                {
                    sideArmObj.transform.position = m_sideArmHolster.transform.position;
                    sideArmObj.transform.rotation = m_sideArmHolster.transform.rotation;
                }
            }

            if (this.HasMainWeapon())
            {
                var mainWeaponObj = this.GetMainWeapon();

                if (m_selectedWeapon != SelectedWeapon.Main && mainWeaponObj != null)
                {
                    mainWeaponObj.transform.position = m_mainWeaponHolster.transform.position;
                    mainWeaponObj.transform.rotation = m_mainWeaponHolster.transform.rotation;
                }
            }

            if (this.HasPackItem())
            {
                var packObj = this.GetPackItem();
                packObj.transform.position = m_packHolster.transform.position;
                packObj.transform.rotation = m_packHolster.transform.rotation;
            }

        }

        public GameObject GetPackItem()
        {
            return m_pack;
        }

        private bool HasPackItem()
        {
            return m_pack != null;
        }

        public bool HasMainWeapon()
        {
            return m_mainWeapon != null;
        }

        public void SetPackItem(GameObject item)
        {
            if (m_pack == null)
            {
                NotificationService.Instance.Info();
                m_pack = item;
            }
        }

        public void SetMainWeapon(GameObject mainWeapon)
        {
            if (m_mainWeapon == null)
            {
                NotificationService.Instance.Info();
                m_mainWeapon = mainWeapon;
            }
        }

        public ActorInventoryState GetActorInventoryState()
        {
            return this.GetNetActorInventory().ToActorInventoryState();
        }

        public NetActorInventory GetNetActorInventory()
        {
            ulong mwNetworkObjId, saNetworkObjId, packNetObjId;
            mwNetworkObjId = saNetworkObjId = packNetObjId = 0;

            if (this.HasMainWeapon())
            {
                var weaponObj = this.GetMainWeapon();
                mwNetworkObjId = weaponObj.GetComponent<NetworkObject>().NetworkObjectId;
            }

            if (this.HasSideArm())
            {
                var sidearmObj = this.GetSideArm();
                saNetworkObjId = sidearmObj.GetComponent<NetworkObject>().NetworkObjectId;
            }

            if (this.HasPackItem())
            {
                var packObj = this.GetPackItem();
                packNetObjId = packObj.GetComponent<NetworkObject>().NetworkObjectId;
            }

            return new NetActorInventory
            {
                MainWeaponType = this.GetWeaponType(SelectedWeapon.Main),
                MainWeaponNetworkObjectId = mwNetworkObjId,
                SideArmType = this.GetWeaponType(SelectedWeapon.Sidearm),
                SidearmNetworkObjectId = saNetworkObjId,
                PackType = this.GetPackType(),
                PackNetworkObjectId = packNetObjId
            };
        }

        public void SetInventoryFromActorInventoryState(ActorInventoryState state)
        {
            if (state.MainWeaponType != this.GetMainWeaponType())
            {
                this.ClearMainWeapon();
            }

            if (state.MainWeapon != null)
            {
                this.SetMainWeapon(state.MainWeapon.gameObject);
            }

            if (state.SideArmType != this.GetSideArmType())
            {
                this.ClearSideArm();
            }

            if (state.SideArm != null)
            {
                this.SetSideArm(state.SideArm.gameObject);
            }

            if (state.PackType != this.GetPackType())
            {
                this.ClearPackItem();
            }

            if (state.Pack != null)
            {
                this.SetPackItem(state.Pack.gameObject);
            }
        }

        public WeaponType GetMainWeaponType()
        {
            return this.HasMainWeapon() ? this.GetMainWeapon().GetComponent<Weapon>().WeaponType : WeaponType.None;
        }

        public PackType GetPackType()
        {
            return this.HasPackItem() ? this.GetPackItem().GetComponent<PickupItem>().PackType : PackType.None;
        }

        public WeaponType GetSideArmType()
        {
            return this.HasSideArm() ? this.GetSideArm().GetComponent<Weapon>().WeaponType : WeaponType.None;
        }

        private WeaponType GetWeaponType(SelectedWeapon selectedWeapon)
        {
            switch (selectedWeapon)
            {
                case SelectedWeapon.None:
                    return WeaponType.None;
                case SelectedWeapon.Main:
                    return this.HasMainWeapon() ? this.GetMainWeapon().GetComponent<Weapon>().WeaponType : WeaponType.None;
                case SelectedWeapon.Sidearm:
                    return this.HasSideArm() ? this.GetSideArm().GetComponent<Weapon>().WeaponType : WeaponType.None;
                case SelectedWeapon.Pack:
                    break;
            }

            return WeaponType.None;
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
                var wpn = m_mainWeapon.GetComponent<Weapon>();

                NotificationService.Instance.Info(wpn.Name);

                m_mainWeapon = null;

                this.OnMainWeaponCleared?.Invoke(this, new OnPickupEventArgs(wpn));

                if (m_selectedWeapon == SelectedWeapon.Main)
                {
                    this.SelectWeapon(SelectedWeapon.None);
                }
            }
        }

        public void ClearPackItem()
        {
            if (m_pack != null)
            {
                var pck = m_pack.GetComponent<PickupItem>();
                NotificationService.Instance.Info(pck.Name);
                m_pack = null;
                this.OnPackCleared?.Invoke(this, new OnPickupEventArgs(pck));
            }
        }

        public void ClearSideArm()
        {
            if (m_sideArm != null)
            {
                var wpn = m_sideArm.GetComponent<Weapon>();

                NotificationService.Instance.Info(wpn.Name);
                m_sideArm = null;

                this.OnSidearmCleared?.Invoke(this, new OnPickupEventArgs(wpn));

                if (m_selectedWeapon == SelectedWeapon.Sidearm)
                {
                    this.SelectWeapon(SelectedWeapon.None);
                }
            }
        }

        public void SelectWeapon(SelectedWeapon selection)
        {
            if (m_selectedWeapon == selection)
            {
                return;
            }

            NotificationService.Instance.Info(selection.ToString());
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
