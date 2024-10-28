using Assets.Scripts.UI.Models;
using Assets.Scripts.UI.Validators;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class Host : GameMenu
    {
        [SerializeField]
        private TMP_InputField m_serverNameInputField;

        [SerializeField]
        private TMP_InputField m_serverPortInputField;

        [SerializeField]
        private TMP_Dropdown m_matchTypeDropDown;

        [SerializeField]
        private TMP_Dropdown m_levelDropDown;


        private static string[] availableScenes = new string[] { "Playground", "VolcanicPlanet01" };

        // Start is called before the first frame update
        protected override void Start()
        {
            m_serverPortInputField.inputValidator = ScriptableObject.CreateInstance<UShortInputValidator>();

            PopulateLevelsDropDown();
            PopulateMatchTypeDropDown();
            base.Start();
        }

        public void LaunchGame()
        {
            Model.MatchSettings.IsHost = true;
            this.GameManager.LaunchGame();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        private void PopulateMatchTypeDropDown()
        {
            m_matchTypeDropDown.options.Clear();

            var values = Enum.GetValues(typeof(MatchType));

            IList matchTypes = values;
            for (int i = 0; i < 1/*matchTypes.Count*/; i++) //Only support Deathmatch at the minute
            {
                object val = matchTypes[i];
                m_matchTypeDropDown.options.Add(new TMP_Dropdown.OptionData(val.ToString()));
            }

            m_matchTypeDropDown.value = -1; //Why do I need to do this?
            m_matchTypeDropDown.value = 0;
        }

        private void PopulateLevelsDropDown()
        {
            m_levelDropDown.options.Clear();

            foreach (var scene in availableScenes)
            {
                m_levelDropDown.options.Add(new TMP_Dropdown.OptionData(scene));
            }

            m_levelDropDown.value = -1; //Why do I need to do this?
            m_levelDropDown.value = 0;
        }

        protected override void UpdateControls(SettingsModel model)
        {
            // TODO : Refactor into its own method / extension
            for (int i = 0; i < m_matchTypeDropDown.options.Count; i++)
            {
                var opts = m_matchTypeDropDown.options[i];
                if (opts.text == model.MatchSettings.MatchType.ToString())
                {
                    m_matchTypeDropDown.value = i;
                    break;
                }
            }

            for (int i = 0; i < m_levelDropDown.options.Count; i++)
            {
                var opts = m_levelDropDown.options[i];
                if (opts.text == model.MatchSettings.Level)
                {
                    m_matchTypeDropDown.value = i;
                    break;
                }
            }

            m_serverNameInputField.text = Model.MatchSettings.ServerName;
            m_serverPortInputField.text = Model.MatchSettings.Port.ToString();
        }

        protected override void UpdateGameSettingsModel()
        {
            if (Model != null)
            {
                var selectedMatchOption = m_matchTypeDropDown.options[m_matchTypeDropDown.value];
                var matchType = Enum.Parse<MatchType>(selectedMatchOption.text);

                Model.MatchSettings.ServerName = m_serverNameInputField.text;
                Model.MatchSettings.Port = ushort.Parse(m_serverPortInputField.text);
                Model.MatchSettings.MatchType = matchType;
                Model.MatchSettings.Level = m_levelDropDown.options[m_levelDropDown.value].text;
            }
        }
    }
}