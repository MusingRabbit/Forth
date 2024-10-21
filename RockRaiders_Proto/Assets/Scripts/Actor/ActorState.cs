using UnityEngine;
using Assets.Scripts;

namespace Assets.Scripts.Actor
{
    public class ActorState : RRMonoBehaviour
    {
        public Team Team { get; set; }

        public SelectedWeapon SelectedWeapon { get; set; }
        public ActorInventory Inventory { get; set; }

        public bool GravBootsEnabled { get; set; }

        public bool IsMoving { get; set; }
        public bool IsFloating { get; set; }

        public bool FeetOnGround { get; set; }

        public bool IsCrouched { get; set; }
        public bool IsMovingForward { get; set; }

        public bool IsDead
        {
            get
            {
                return m_health.State == ActorHealthState.Dying || m_health.State == ActorHealthState.Dead ;
            }
        }

        public int Health
        {
            get
            {
                return m_health?.Hitpoints.Current ?? 0;
            }
        }


        private ActorHealth m_health;

        public ActorState()
        {
            this.SelectedWeapon = SelectedWeapon.None;
        }

        public override void Initialise()
        {
            this.Inventory = this.GetComponent<ActorInventory>();
            m_health = this.GetComponent<ActorHealth>();
        }

        private void Start()
        {
            this.Initialise();
        }

        public override void Reset()
        {
            this.SelectedWeapon = SelectedWeapon.None;
            this.GravBootsEnabled = true;
            this.IsFloating = false;
            this.IsMoving = false;
            this.IsCrouched = false;
            this.FeetOnGround = false;
            this.IsMovingForward = false;
            this.Team = Team.None;
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
