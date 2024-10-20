using Assets.Scripts.Managers;
using Assets.Scripts.UI.Models;
using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class GameMenu : MonoBehaviour
    {
        [SerializeField]
        private GameManager m_gameManager;

        private SettingsModel m_model;

        public SettingsModel Model
        {
            get
            {
                return m_model;
            }
            set
            {
                m_model = value;
                this.UpdateControls(value);
            }
        }

        protected GameManager GameManager
        {
            get
            {
                return m_gameManager;
            }
        }

        protected virtual void Start()
        {
            this.Model = m_gameManager.Settings;
        }

        protected virtual void Update()
        {
            try
            {
                if (this.Model != null)
                {
                    this.UpdateGameSettingsModel();
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("Failed to update game model. " + ex.Message);
            }
        }

        protected abstract void UpdateControls(SettingsModel model);
        protected abstract void UpdateGameSettingsModel();
    }
}
