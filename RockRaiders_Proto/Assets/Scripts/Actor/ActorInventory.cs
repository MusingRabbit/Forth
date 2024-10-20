using UnityEngine;

namespace Assets.Scripts
{
    public enum CurrentSelection
    {
        None = 0,
        MainWeapon = 1,
        SideArm = 2
    }

    public class ActorInventory : RRMonoBehaviour
    {
        [SerializeField]
        private CurrentSelection m_currentSelection;

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

        private void Update()
        {
            this.UpdateStoredItemWorldPos();
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

        private void ConfigureRigidBodyOnPickup(GameObject weapon)
        {
            weapon.GetComponent<Rigidbody>().isKinematic = true;
            weapon.GetComponent<Rigidbody>().detectCollisions = false;
        }

        private void ConfigureRigidBodyOnDrop(GameObject weapon)
        {
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon.GetComponent<Rigidbody>().detectCollisions = true;
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
            this.ConfigureRigidBodyOnDrop(m_mainWeapon);
            m_mainWeapon = null;

            if (m_currentSelection == CurrentSelection.MainWeapon)
            {
                this.SelectWeapon(CurrentSelection.None);
            }
        }

        public void ClearSideArm()
        {
            this.ConfigureRigidBodyOnDrop(m_sideArm);
            m_sideArm = null;

            if (m_currentSelection == CurrentSelection.SideArm)
            {
                this.SelectWeapon(CurrentSelection.None);
            }

        }

        public void SelectWeapon(CurrentSelection selection)
        {
            m_currentSelection = CurrentSelection.None;

            switch (selection)
            {
                case CurrentSelection.MainWeapon:

                    if (m_mainWeapon != null)
                    {
                        m_currentSelection = selection;
                    }
                    break;
                case CurrentSelection.SideArm:

                    if (m_sideArm != null)
                    {
                        m_currentSelection = selection;
                    }
                    break;
            }
        }
    }
}
