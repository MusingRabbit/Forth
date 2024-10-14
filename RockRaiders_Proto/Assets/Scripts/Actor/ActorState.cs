using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorState 
    {
        public SelectedWeapon SelectedWeapon { get; set; }

        public ActorInventory Inventory { get; set; }

        public bool IsMoving { get; set; }
        public bool IsFloating { get; set; }
        public bool IsCrouched { get; set; }
        public bool IsMovingForward { get; set; }

        public ActorState()
        {
            this.SelectedWeapon = SelectedWeapon.None;
            this.Inventory = new ActorInventory();
        }

        public GameObject GetSelectedWeapon()
        {
            switch (this.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return null;
                case SelectedWeapon.Main:
                    return this.Inventory.GetMainWeapon();
                case SelectedWeapon.Sidearm:
                    return this.Inventory.GetSideArm();
                case SelectedWeapon.Pack:
                    return null;
            }

            return null;
        }

        public void SelectWeapon(SelectedWeapon weapon)
        {
            var selectedWeapon = this.GetSelectedWeapon();

            switch (weapon)
            {
                case SelectedWeapon.Main:
                    if (this.Inventory.HasMainWeapon())
                    {
                        this.SelectedWeapon = SelectedWeapon.Main;
                    }
                    break;
                case SelectedWeapon.Sidearm:
                    if (this.Inventory.HasSideArm())
                    {
                        this.SelectedWeapon = SelectedWeapon.Sidearm;
                    }
                    break;
                case SelectedWeapon.None:
                    this.SelectedWeapon = SelectedWeapon.None;
                    break;
            }
        }
    }
}
