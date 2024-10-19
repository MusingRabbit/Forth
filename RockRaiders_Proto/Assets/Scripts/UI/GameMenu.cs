using Assets.Scripts.Managers;
using Assets.Scripts.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class GameMenu : MonoBehaviour
    {
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

        protected virtual void Start()
        {
            this.Model = GameManager.Instance.Settings;
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
