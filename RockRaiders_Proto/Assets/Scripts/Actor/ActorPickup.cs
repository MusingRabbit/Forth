using Assets.Scripts.Events;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using Assets.Scripts.UI;
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
    public class ActorPickup : MonoBehaviour
    {
        public event EventHandler<OnPickupEventArgs> OnItemPickedUp;
        public event EventHandler<OnPickupEventArgs> OnItemDropped;

        private ActorInventory m_inventory;
        private ActorCrosshair m_crosshair;

        private Timer m_dropTimer;

        [SerializeField]
        private bool m_canPickup;

        [SerializeField]
        private double m_dropTimeOut;

        [SerializeField]
        private float m_dropForce;

        public ActorPickup()
        {
            m_canPickup = true;
            m_dropForce = 3.0f;
            m_dropTimeOut = 1.0f;

            m_dropTimer = new Timer();
        }

        private void Start()
        {
            m_inventory = this.GetComponent<ActorState>().Inventory;
            m_crosshair = this.GetComponent<ActorCrosshair>();

            m_inventory.OnMainWeaponCleared += Inventory_OnMainWeaponCleared;
            m_inventory.OnSidearmCleared += Inventory_OnSidearmCleared;

            if (m_canPickup)
            {
                m_dropTimer.SetTimeSpan(TimeSpan.FromSeconds(m_dropTimeOut));
                m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
                m_dropTimer.AutoReset = false;
            }
        }

        private void Update()
        {
            m_dropTimer.Tick();
        }

        public bool DropSelectedWeapon()
        {
            var weaponObj = m_inventory.GetSelectedWeapon();

            if (weaponObj != null)
            {
                switch (m_inventory.SelectedWeapon)
                {
                    case SelectedWeapon.None:
                        break;
                    case SelectedWeapon.Main:
                        m_inventory.ClearMainWeapon();
                        break;
                    case SelectedWeapon.Sidearm:
                        m_inventory.ClearSideArm();
                        break;
                    case SelectedWeapon.Pack:
                        break;
                }

                m_inventory.SelectWeapon(SelectedWeapon.None);


                this.DropWeapon(weaponObj.GetComponent<Weapon>());

                NotificationService.Instance.Info(weaponObj.ToString());

                

                return true;
            }

            return false;
        }

        private void DropWeapon(Weapon weapon)
        {
            NotificationService.Instance.Info(weapon.Name);

            m_dropTimer.ResetTimer();
            m_dropTimer.Start();
            m_canPickup = false;

            var rb = weapon.GetComponent<Rigidbody>();
            
            rb.AddForce(weapon.transform.forward.normalized * m_dropForce, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)), ForceMode.Impulse);
            weapon.Owner = null;
            weapon.Crosshair = null;
            weapon.SetDropped();

            this.OnItemDropped?.Invoke(this, new OnPickupEventArgs(weapon));
        }

        public bool PickupWeapon(Weapon weapon)
        {
            if (m_canPickup)
            {
                NotificationService.Instance.Info(weapon.Name.ToString());

                switch (weapon.WeaponSlot)
                {
                    case WeaponSlot.Main:
                        if (!m_inventory.HasMainWeapon())
                        {
                            m_inventory.SetMainWeapon(weapon.gameObject);
                        }

                        break;
                    case WeaponSlot.Sidearm:
                        if (!m_inventory.HasSideArm())
                        {
                            m_inventory.SetSideArm(weapon.gameObject);
                        }

                        break;
                }

                weapon.Crosshair = m_crosshair;
                weapon.Owner = this.gameObject;
                weapon.SetPickedUp();

                this.OnItemPickedUp?.Invoke(this, new OnPickupEventArgs(weapon));

                return true;
            }

            return false;
        }


        private void DropTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            NotificationService.Instance.Info();

            m_dropTimer.Stop();
            m_dropTimer.ResetTimer();

            m_canPickup = true;
        }

        private void Inventory_OnSidearmCleared(object sender, OnPickupEventArgs e)
        {
            this.DropWeapon((Weapon)e.Item);
        }

        private void Inventory_OnMainWeaponCleared(object sender, OnPickupEventArgs e)
        {
            this.DropWeapon((Weapon)e.Item);
        }

    }
}
