using Assets.Scripts.Actor;
using Assets.Scripts.Managers;
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
        private GameObject m_pauseMenu;

        [SerializeField]
        private ActorController m_actor;

        [SerializeField]
        private Text m_txtGravBootsEnabled;

        [SerializeField]
        private Text m_txtHitpointsValue;

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
        }

        // Update is called once per frame
        void Update()
        {
            this.UpdateGravBootsStatus();
            this.UpdateHealthStatus();

            if (m_gameManager.PlayerPaused)
            {
                m_hud.SetActive(false);
                m_pauseMenu.SetActive(true);
            }
            else
            {
                m_hud.SetActive(true);
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
            var txtTint = hp > 30 ? Color.green : Color.red;
            m_txtHitpointsValue.text = hp > 0 ? hp.ToString() + "%" : "Unknown";
            m_txtHitpointsValue.color = txtTint;
        }
    }
}