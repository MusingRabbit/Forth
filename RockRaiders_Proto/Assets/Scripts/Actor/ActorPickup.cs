using Assets.Scripts.Events;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Actor
{
    public class ActorPickup : RRMonoBehaviour
    {
        /// <summary>
        /// Fired when item has been picked up
        /// </summary>
        public event EventHandler<OnPickupEventArgs> OnItemPickedUp;

        /// <summary>
        /// Fired when item has been dropped
        /// </summary>
        public event EventHandler<OnPickupEventArgs> OnItemDropped;

        /// <summary>
        /// Stores a reference to the actors' state
        /// </summary>
        private ActorState m_actorState;

        /// <summary>
        /// Stores a reference to the actors' crosshair
        /// </summary>
        private ActorCrosshair m_crosshair;

        /// <summary>
        /// The drop timer determines how long before the actor may pick up another weapon after dropping
        /// </summary>
        private Timer m_dropTimer;

        /// <summary>
        /// The drop timer determines how long before the actor may pick up another pack after dropping
        /// </summary>
        private Timer m_packDropTimer;

        /// <summary>
        /// Stores whether the actor can pick up weapons 
        /// </summary>
        [SerializeField]
        private bool m_canPickup;

        /// <summary>
        /// Stores whether the actor can pick up packs
        /// </summary>
        private bool m_canPickupPack;

        /// <summary>
        /// The amount of time after dropping a weapon, can the actor pick up another weapon
        /// </summary>
        [SerializeField]
        private double m_dropTimeOut;

        /// <summary>
        /// The amount of force exerted on the object / pack as it is dropped.
        /// </summary>
        [SerializeField]
        private float m_dropForce;

        /// <summary>
        /// Gets the actors inventory
        /// </summary>
        private ActorInventory ActorInventory
        {
            get
            {
                return m_actorState.Inventory;
            }
        }


        public ActorPickup()
        {
            m_canPickup = true;
            m_canPickupPack = true;
            m_dropForce = 1.0f;
            m_dropTimeOut = 1.0f;

            m_dropTimer = new Timer();
            m_packDropTimer = new Timer();
        }

        /// <summary>
        /// Start - called before first frame in scene
        /// </summary>
        private void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            m_dropTimer.Tick();
            m_packDropTimer.Tick();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_actorState = this.GetComponent<ActorState>();
            m_crosshair = this.GetComponent<ActorCrosshair>();

            this.ActorInventory.OnMainWeaponCleared += Inventory_OnMainWeaponCleared;
            this.ActorInventory.OnSidearmCleared += Inventory_OnSidearmCleared;
            this.ActorInventory.OnPackCleared += Inventory_OnPackCleared;

            m_dropTimer.SetTimeSpan(TimeSpan.FromSeconds(m_dropTimeOut));
            m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
            m_dropTimer.AutoReset = false;

            m_packDropTimer.SetTimeSpan(TimeSpan.FromSeconds(m_dropTimeOut));
            m_dropTimer.OnTimerElapsed += this.PackDropTimer_OnTimerElapsed;
            m_dropTimer.AutoReset = false;
        }

        /// <summary>
        /// Resets actor pickup component
        /// </summary>
        public override void Reset()
        {
            m_canPickup = true;
            m_canPickupPack = true;
            m_dropForce = 3.0f;
            m_dropTimeOut = 1.0f;

            m_dropTimer = new Timer();
            m_packDropTimer = new Timer();
        }

        /// <summary>
        /// Drops the current pack
        /// </summary>
        /// <param name="suppressNotification">Suppress events following drop</param>
        /// <returns>Weapon dropped? (true/false)</returns>
        public bool DropCurrentPack(bool suppressNotification = false)
        {
            m_packDropTimer.ResetTimer();
            m_packDropTimer.Start();
            m_canPickupPack = false;

            var packObj = this.ActorInventory.GetPackItem();

            if (packObj != null)
            {
                this.ActorInventory.ClearPackItem();

                this.DropPickupItem(packObj.GetComponent<PickupItem>(), suppressNotification);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Drops the currently selected weapon
        /// </summary>
        /// <param name="suppressNotifications">suppress events following drop</param>
        /// <returns>Weapon dropped? (true/false)</returns>
        public bool DropSelectedWeapon(bool suppressNotifications = false)
        {
            var weaponObj = this.ActorInventory.GetSelectedWeapon();

            if (weaponObj != null)
            {
                switch (this.ActorInventory.SelectedWeapon)
                {
                    case SelectedWeapon.None:
                        break;
                    case SelectedWeapon.Main:
                        this.ActorInventory.ClearMainWeapon();
                        break;
                    case SelectedWeapon.Sidearm:
                        this.ActorInventory.ClearSideArm();
                        break;
                    case SelectedWeapon.Pack:
                        this.ActorInventory.ClearPackItem();
                        break;
                }

                this.ActorInventory.SelectWeapon(SelectedWeapon.None);

                this.DropPickupItem(weaponObj.GetComponent<Weapon>(), suppressNotifications);

                NotificationService.Instance.Info(weaponObj.ToString());

                return true;
            }

            return false;
        }

        /// <summary>
        /// Drop pickup item
        /// </summary>
        /// <param name="pickupItem"></param>
        /// <param name="suppressNotifications"></param>
        private void DropPickupItem(PickupItem pickupItem, bool suppressNotifications = false)
        {
            NotificationService.Instance.Info(pickupItem.Name);

            m_dropTimer.ResetTimer();
            m_dropTimer.Start();
            m_canPickup = false;

            var rb = pickupItem.GetComponent<Rigidbody>();

            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(pickupItem.transform.forward.normalized * m_dropForce, ForceMode.Force);
            rb.AddTorque(new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(0f, 0.01f), Random.Range(-0.05f, 0.05f)), ForceMode.Force);
            
            pickupItem.SetDropped(suppressNotifications);

            pickupItem.Owner = null;

            var wpn = pickupItem is Weapon ? pickupItem as Weapon : null;

            if (wpn != null)
            {
                wpn.Crosshair = null;
            }

            this.OnItemDropped?.Invoke(this, new OnPickupEventArgs(pickupItem));
        }

        /// <summary>
        /// Pickup pickup item (pack)
        /// </summary>
        /// <param name="item">Pack pickup item</param>
        /// <param name="suppressNotifications">Suppress events following pickup</param>
        /// <returns>pickup success? (true/false)</returns>
        public bool PickupPack(PickupItem item, bool suppressNotifications = false)
        {
            if (m_canPickupPack)
            {
                NotificationService.Instance.Info(item.Name);
                this.ActorInventory.SetPackItem(item.gameObject);

                item.Owner = this.gameObject;
                item.SetPickedUp(suppressNotifications);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pickup pickup item (weapon)
        /// </summary>
        /// <param name="item">Pack weapon item</param>
        /// <param name="suppressNotifications">Suppress events following pickup</param>
        /// <returns>pickup success? (true/false)</returns>
        public bool PickupWeapon(Weapon weapon, bool suppressNotifications)
        {
            if (m_canPickup)
            {
                NotificationService.Instance.Info(weapon.Name);

                switch (weapon.WeaponSlot)
                {
                    case WeaponSlot.Main:
                        if (!this.ActorInventory.HasMainWeapon())
                        {
                            this.ActorInventory.SetMainWeapon(weapon.gameObject);
                        }

                        break;
                    case WeaponSlot.Sidearm:
                        if (!this.ActorInventory.HasSideArm())
                        {
                            this.ActorInventory.SetSideArm(weapon.gameObject);
                        }

                        break;
                }

                weapon.Crosshair = m_crosshair;
                weapon.Owner = this.gameObject;
                weapon.SetPickedUp(suppressNotifications);

                this.OnItemPickedUp?.Invoke(this, new OnPickupEventArgs(weapon));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Triggered on the lapse of m_dropTimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            NotificationService.Instance.Info();

            m_dropTimer.Stop();
            m_dropTimer.ResetTimer();

            m_canPickup = true;
        }

        /// <summary>
        /// Triggered on the lapse of m_packDropTimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackDropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_packDropTimer.Stop();
            m_packDropTimer.ResetTimer();

            m_canPickupPack = true;
        }

        /// <summary>
        /// Triggered when the inventory pack slot has been cleared
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_OnPackCleared(object sender, OnPickupEventArgs e)
        {
            this.DropPickupItem(e.Item);
        }

        /// <summary>
        /// Triggered when the inventory sidearm slot has been cleared
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_OnSidearmCleared(object sender, OnPickupEventArgs e)
        {
            this.DropPickupItem(e.Item);
        }

        /// <summary>
        /// Triggered when the inventory main weapon slot has been cleared.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_OnMainWeaponCleared(object sender, OnPickupEventArgs e)
        {
            this.DropPickupItem(e.Item);
        }
    }
}
