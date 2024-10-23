using Assets.Scripts.Actor;
using Assets.Scripts.Managers;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIGameOverlay : MonoBehaviour
    {
        [SerializeField]
        private GameManager m_gameManager;

        [SerializeField]
        private GameObject m_hud;

        [SerializeField]
        private GameObject m_ded;

        [SerializeField]
        private GameObject m_pauseMenu;

        [SerializeField]
        private ActorController m_actor;

        [SerializeField]
        private Text m_txtGravBootsEnabled;

        [SerializeField]
        private Text m_txtHitpointsValue;

        [SerializeField]
        private Text m_txtAmmoCountValue;

        [SerializeField]
        private TMP_Text m_txtNotificationText;

        private ActorState m_actorState;

        public ActorController Actor
        {
            get
            {
                return m_actor;
            }
            set
            {
                m_actor = value;
                m_actorState = m_actor.GetComponent<ActorState>();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (m_gameManager == null)
            {
                m_gameManager = GameManager.Instance;
            }

            NotificationService.Instance.OnPlayerKilled += this.NotificationService_OnPlayerKilled;
        }



        // Update is called once per frame
        void Update()
        {
            this.UpdateGravBootsStatus();
            this.UpdateHealthStatus();
            this.UpdateAmmoStatus();

            if (m_gameManager.PlayerPaused)
            {
                m_hud.SetActive(false);
                m_pauseMenu.SetActive(true);
            }
            else if (m_gameManager.PlayerAwaitingRespawn)
            {
                m_ded.SetActive(true);
                m_hud.SetActive(false);
            }
            else
            {
                m_hud.SetActive(true);
                m_ded.SetActive(false);
                m_pauseMenu.SetActive(false);
            }
        }

        private void UpdateGravBootsStatus()
        {
            string txtVal = "Unknown";
            txtVal = m_actorState?.GravBootsEnabled ?? false ? "True" : "False";
            var txtTint = m_actorState?.GravBootsEnabled ?? false ? Color.green : Color.red;
            m_txtGravBootsEnabled.text = txtVal;
            m_txtGravBootsEnabled.color = txtTint;
        }

        private void UpdateHealthStatus()
        {
            var hp = m_actorState?.Health ?? -1;
            var txtTint = GetColourFromPercentage(hp);
            m_txtHitpointsValue.text = hp > 0 ? hp.ToString() + "%" : "Unknown";
            m_txtHitpointsValue.color = txtTint;
        }

        private Color GetColourFromPercentage(float percentage)
        {
            if (percentage < 30)
            {
                return Color.red;
            }

            if (percentage < 50)
            {
                return Color.yellow;
            }

            return Color.green;
        }

        private void UpdateAmmoStatus()
        {
            var weaponObj = m_actorState.Inventory.GetSelectedWeapon();
            var ammoCount = -1;
            var maxAmmo = 0;
            var ammoPercentage = 0.0f;

            if (weaponObj != null)
            {
                var wpn = weaponObj.GetComponent<Weapon>();
                ammoCount = wpn.Ammo;
                ammoPercentage = ((float)wpn.Ammo / (float)wpn.MaxAmmo) * 100.0f;
                maxAmmo = wpn.MaxAmmo;
            }

            m_txtAmmoCountValue.text = ammoCount > -1 ? ammoCount.ToString() : maxAmmo == -1 ? "Infinite" : "Unknown";
            m_txtAmmoCountValue.color = this.GetColourFromPercentage(ammoPercentage);
        }

        private void NotificationService_OnPlayerKilled(object sender, Events.OnNotificationEventArgs e)
        {
            m_txtNotificationText.text = e.Data.Message + "\n" + m_txtNotificationText.text;
        }
    }
}