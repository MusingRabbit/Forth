using Assets.Scripts.Managers;
using Assets.Scripts.UI.Models;
using Assets.Scripts.UI.Validators;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class Join : GameMenu
    {
        [SerializeField]
        private TMP_InputField m_serverIpInputField;

        [SerializeField]
        private TMP_InputField m_portInputField;


        public Join()
        {
            
        }

        protected override void Start()
        {
            m_portInputField.inputValidator = ScriptableObject.CreateInstance<UShortInputValidator>();

            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        public void LaunchGame()
        {
            this.Model.MatchSettings.IsHost = false;
            this.GameManager.LaunchGame();
        }

        protected override void UpdateControls(SettingsModel model)
        {
            m_portInputField.text = model.MatchSettings.Port.ToString();
            m_serverIpInputField.text = model.MatchSettings.ServerIP;
        }

        protected override void UpdateGameSettingsModel()
        {
            this.Model.MatchSettings.Port = ushort.Parse(m_portInputField.text);
            this.Model.MatchSettings.ServerIP = m_serverIpInputField.text;
        }
    }
}
