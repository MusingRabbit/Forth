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
        private TMP_InputField m_scoreLimitField;

        [SerializeField]
        private TMP_Dropdown m_matchTypeDropDown;

        [SerializeField]
        private TMP_Dropdown m_levelDropDown;

        private MatchType m_oldMatchType;


        private static string[] availableScenes = new string[] { "Playground", "VolcanicPlanet01", "Morpheus", "SS_Miner" };

        // Start is called before the first frame update
        protected override void Start()
        {
            m_serverPortInputField.inputValidator = ScriptableObject.CreateInstance<UShortInputValidator>();

            PopulateLevelsDropDown();
            PopulateMatchTypeDropDown();
            base.Start();
        }

        protected override void Update()
        {
            m_oldMatchType = Model.Session.MatchSettings.MatchType;

            base.Update();

            if (m_oldMatchType != Model.Session.MatchSettings.MatchType)
            {
                switch (Model.Session.MatchSettings.MatchType)
                {
                    case MatchType.Deathmatch:
                        m_scoreLimitField.text = "15";
                        break;
                    case MatchType.TeamDeathmatch:
                        m_scoreLimitField.text = "30";
                        break;
                    case MatchType.CaptureTheFlag:
                        m_scoreLimitField.text = "3";
                        break;
                }

                this.UpdateGameSettingsModel();
            }
        }

        public void LaunchGame()
        {
            Model.Session.IsHost = true;
            this.GameManager.LaunchGame();
        }

        private void PopulateMatchTypeDropDown()
        {
            m_matchTypeDropDown.options.Clear();

            var values = Enum.GetValues(typeof(MatchType));

            IList matchTypes = values;
            for (int i = 0; i < matchTypes.Count; i++) //Only support Deathmatch at the minute
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

        protected override void UpdateControls(SettingsModel settings)
        {
            // TODO : Refactor into its own method / extension
            for (int i = 0; i < m_matchTypeDropDown.options.Count; i++)
            {
                var opts = m_matchTypeDropDown.options[i];
                if (opts.text == settings.Session.MatchSettings.ToString())
                {
                    m_matchTypeDropDown.value = i;
                    break;
                }
            }

            for (int i = 0; i < m_levelDropDown.options.Count; i++)
            {
                var opts = m_levelDropDown.options[i];
                if (opts.text == settings.Session.Level)
                {
                    m_matchTypeDropDown.value = i;
                    break;
                }
            }

            m_serverNameInputField.text = Model.Session.ServerName;
            m_serverPortInputField.text = Model.Session.Port.ToString();
        }

        protected override void UpdateGameSettingsModel()
        {
            if (Model != null)
            {
                var selectedMatchOption = m_matchTypeDropDown.options[m_matchTypeDropDown.value];
                var matchType = Enum.Parse<MatchType>(selectedMatchOption.text);
                var scoreLimit = int.Parse(m_scoreLimitField.text);

                Model.Session.ServerName = m_serverNameInputField.text;
                Model.Session.Port = ushort.Parse(m_serverPortInputField.text);

                Model.Session.MatchSettings = new MatchSettings { MatchType = matchType, ScoreLimit = scoreLimit, TimeLimit = TimeSpan.Zero };
                Model.Session.Level = m_levelDropDown.options[m_levelDropDown.value].text;
            }
        }
    }
}