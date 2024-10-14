using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public enum CurrentSelection
    {
        None = 0,
        MainWeapon = 1,
        SideArm = 2
    }

    public class ActorInventory
    {
        private CurrentSelection m_currentSelection;

        private GameObject m_mainWeapon;
        private GameObject m_sideArm;
        private GameObject m_pack;

        public bool HasMainWeapon()
        {
            return m_mainWeapon != null;
        }

        public void SetMainWeapon(GameObject mainWeapon)
        {
            if (m_mainWeapon == null)
            {
                m_mainWeapon = mainWeapon;
            }
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
            m_mainWeapon = null;

            if (m_currentSelection == CurrentSelection.MainWeapon)
            {
                this.SelectWeapon(CurrentSelection.None);
            }
        }

        public void ClearSideArm()
        {
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
