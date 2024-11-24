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
    /// <summary>
    /// Network Data for actor inventory
    /// </summary>
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

    /// <summary>
    /// Actor inventory state component
    /// </summary>
    public class ActorInventoryState : IEquatable<ActorInventoryState>
    {
        /// <summary>
        /// Gets or sets the main weapon type
        /// </summary>
        public WeaponType MainWeaponType { get; set; }
        
        /// <summary>
        /// Gets or sets the main weapon
        /// </summary>
        public Weapon MainWeapon { get; set; }

        /// <summary>
        /// Gets or sets the sidearm type
        /// </summary>
        public WeaponType SideArmType { get; set; }

        /// <summary>
        /// Gets or sets the sidearm
        /// </summary>
        public Weapon SideArm { get; set; }

        /// <summary>
        /// Gets or sets the pack type.
        /// </summary>
        public PackType PackType { get; set; }

        /// <summary>
        /// Gets or sets the pack.
        /// </summary>
        public PickupItem Pack { get; set; }

        /// <summary>
        /// Equality check 
        /// </summary>
        /// <param name="other">rhs actor inventory</param>
        /// <returns>Is Equal? (true/false)</returns>
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

    /// <summary>
    /// Actor inventory
    /// </summary>
    public class ActorInventory : RRMonoBehaviour
    {
        /// <summary>
        /// Fired when main weapon has been cleared from the inventory.
        /// </summary>
        public event EventHandler<OnPickupEventArgs> OnMainWeaponCleared;

        /// <summary>
        /// Fired when sidearm has been cleared from the inventory.
        /// </summary>
        public event EventHandler<OnPickupEventArgs> OnSidearmCleared;

        /// <summary>
        /// Fired when pack has been cleared from the actor inventory.
        /// </summary>
        public event EventHandler<OnPickupEventArgs> OnPackCleared;

        /// <summary>
        /// The primary / main weapon pickup game object
        /// </summary>
        [SerializeField]
        private GameObject m_mainWeapon;

        /// <summary>
        /// The primary / main weapon holster
        /// </summary>
        [SerializeField]
        private GameObject m_mainWeaponHolster;

        /// <summary>
        /// The sidearm pickup game object
        /// </summary>
        [SerializeField]
        private GameObject m_sideArm;

        /// <summary>
        /// The sidearm holseter
        /// </summary>
        [SerializeField]
        private GameObject m_sideArmHolster;

        /// <summary>
        /// The pack pickup game object
        /// </summary>
        [SerializeField]
        private GameObject m_pack;

        /// <summary>
        /// The pack pickup holster
        /// </summary>
        [SerializeField]
        private GameObject m_packHolster;

        /// <summary>
        /// The currently selected weapon.
        /// </summary>
        [SerializeField]
        private SelectedWeapon m_selectedWeapon;

        /// <summary>
        /// Gets or sets the currently selected weapon
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorInventory()
        {
            m_selectedWeapon = SelectedWeapon.None;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        public override void Initialise()
        {
            m_selectedWeapon = SelectedWeapon.None;
        }

        /// <summary>
        /// Called every physics step
        /// </summary>
        private void LateUpdate()
        {
            this.UpdateStoredItemWorldPos();
        }

        /// <summary>
        /// Called every frame 
        /// </summary>
        private void Update()
        {
            
        }

        /// <summary>
        /// Resets actor inventory
        /// </summary>
        public override void Reset()
        {
            this.ClearMainWeapon();
            this.ClearSideArm();
            m_selectedWeapon = SelectedWeapon.None;
        }

        /// <summary>
        /// Updates the world position of all stored items
        /// </summary>
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

        /// <summary>
        /// Get the current pack pickup item. Returns null if none exist.
        /// </summary>
        /// <returns></returns>
        public GameObject GetPackItem()
        {
            return m_pack;
        }

        /// <summary>
        /// Checks to see whether pack item exists within inventory
        /// </summary>
        /// <returns></returns>
        private bool HasPackItem()
        {
            return m_pack != null;
        }

        /// <summary>
        /// Checks to see whether main weapon exists within inventory
        /// </summary>
        /// <returns></returns>
        public bool HasMainWeapon()
        {
            return m_mainWeapon != null;
        }

        /// <summary>
        /// Sets the pack item pickup game object in the actor inventory
        /// </summary>
        /// <param name="item"></param>
        public void SetPackItem(GameObject item)
        {
            if (m_pack == null)
            {
                NotificationService.Instance.Info();
                m_pack = item;
            }
        }

        /// <summary>
        /// Sets the main weapon game object
        /// </summary>
        /// <param name="mainWeapon">Pickup Game Object</param>
        public void SetMainWeapon(GameObject mainWeapon)
        {
            if (m_mainWeapon == mainWeapon)
            {
                return;
            }
            else if (m_mainWeapon != null)
            {
                ClearMainWeapon();
            }

            if (m_mainWeapon == null)
            {
                NotificationService.Instance.Info();
                m_mainWeapon = mainWeapon;
                var pickupItem = m_mainWeapon.GetComponent<PickupItem>();
                pickupItem.SetPickedUp(true);
            }
        }

        /// <summary>
        /// Gets the actor inventory state
        /// </summary>
        /// <returns>Actor Inventory State</returns>
        public ActorInventoryState GetActorInventoryState()
        {
            return this.GetNetActorInventory().ToActorInventoryState();
        }

        /// <summary>
        /// Gets the actor inventory state network data
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the actor inventory based upon the provided inventory state.
        /// </summary>
        /// <param name="state"></param>
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

        /// <summary>
        /// Gets the main weapon type.
        /// </summary>
        /// <returns></returns>
        public WeaponType GetMainWeaponType()
        {
            return this.HasMainWeapon() ? this.GetMainWeapon().GetComponent<Weapon>().WeaponType : WeaponType.None;
        }

        /// <summary>
        /// Gets the current pack type.
        /// </summary>
        /// <returns></returns>
        public PackType GetPackType()
        {
            return this.HasPackItem() ? this.GetPackItem().GetComponent<PickupItem>().PackType : PackType.None;
        }

        /// <summary>
        /// Gets the side arm type.
        /// </summary>
        /// <returns></returns>
        public WeaponType GetSideArmType()
        {
            return this.HasSideArm() ? this.GetSideArm().GetComponent<Weapon>().WeaponType : WeaponType.None;
        }

        /// <summary>
        /// Gets the weapon type for the selected weapon
        /// </summary>
        /// <param name="selectedWeapon"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the main weapon
        /// </summary>
        /// <returns></returns>
        public GameObject GetMainWeapon()
        {
            return m_mainWeapon;
        }

        /// <summary>
        /// Checks whether inventory has a sidearm
        /// </summary>
        /// <returns></returns>
        public bool HasSideArm()
        {
            return m_sideArm != null;
        }

        /// <summary>
        /// Sets the inventory sidearm slot.
        /// </summary>
        /// <param name="sideArm"></param>
        public void SetSideArm(GameObject sideArm)
        {
            if (m_sideArm == sideArm)
            {
                return;
            }
            else if (m_sideArm != null)
            {
                ClearSideArm();
            }

            if (m_sideArm == null)
            {
                m_sideArm = sideArm;
                NotificationService.Instance.Info();
                var pickupItem = m_sideArm.GetComponent<PickupItem>();
                pickupItem.SetPickedUp(true);
            }
        }

        /// <summary>
        /// Gets the sidearm from this inventory
        /// </summary>
        /// <returns></returns>
        public GameObject GetSideArm()
        {
            return m_sideArm;
        }

        /// <summary>
        /// Clears the main weapon slot within this inventory
        /// </summary>
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

        /// <summary>
        /// Clears the pack item within this inventory
        /// </summary>
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

        /// <summary>
        /// Clears the sidearm slot within this inventory
        /// </summary>
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

        /// <summary>
        /// Selects a weapon from within this inventory
        /// </summary>
        /// <param name="selection"></param>
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

        /// <summary>
        /// Gets the currently selected weapon
        /// </summary>
        /// <returns></returns>
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
