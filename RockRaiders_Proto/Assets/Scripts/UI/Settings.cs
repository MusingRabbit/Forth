using Assets.Scripts.Data;
using Assets.Scripts.UI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class Settings : GameMenu
    {
        [SerializeField]
        private TMP_Dropdown m_screenResDropdown;

        [SerializeField]
        private Toggle m_fullScreenToggle;

        [SerializeField]
        private TMP_InputField m_playerNameInput;

        [SerializeField]
        private Scrollbar m_soundVolume;

        [SerializeField]
        private Scrollbar m_musicVolume;

        private SettingsRepository m_settingsRepo;

        public Settings()
        {
            m_settingsRepo = new SettingsRepository();
        }


        // Start is called before the first frame update
        protected override void Start()
        {
            PopulateScreenResolutionsDropDown();
            base.Start();

            if (Model.Game.Resolution == string.Empty)
            {
                m_screenResDropdown.value = -1;
                m_screenResDropdown.value = 0;
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            var oldResolution = Model.Game.Resolution;
            var oldFullScreenMode = Model.Game.FullScreen;

            base.Update();

            if (oldResolution != Model.Game.Resolution)
            {
                SelectScreenResolution(Model.Game.Resolution);
            }
        }

        public void FullScreenToggle()
        {
            Model.Game.FullScreen = !Model.Game.FullScreen;
            SetScreenResolution(Model.Game.Resolution, Model.Game.FullScreen);
        }

        private void PopulateScreenResolutionsDropDown()
        {
            m_screenResDropdown.options.Clear();

            foreach (var res in Screen.resolutions)
            {
                m_screenResDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width}x{res.height}@{res.refreshRateRatio}"));
            }
        }

        public static void SetScreenResolution(string resolution, bool fullScreen = false)
        {
            var tokens = resolution.Split('x', '@');
            var width = int.Parse(tokens[0]);
            var height = int.Parse(tokens[1]);
            var refresh = float.Parse(tokens[2]);

            var fsMode = fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

            Screen.SetResolution(width, height, fsMode, new RefreshRate { numerator = (uint)Mathf.Round(refresh), denominator = 1 });
        }

        private void SelectScreenResolution(string resolution)
        {
            int val = -1;

            for (int i = 0; i < m_screenResDropdown.options.Count; i++)
            {
                var opts = m_screenResDropdown.options[i];

                if (opts.text == resolution)
                {
                    val = i;
                    break;
                }
            }

            if (val != -1)
            {
                m_screenResDropdown.value = val;
                SetScreenResolution(resolution, Model.Game.FullScreen);
            }
            else
            {
                m_screenResDropdown.value = val;
                m_screenResDropdown.value = 0;
            }


        }

        protected override void UpdateControls(SettingsModel model)
        {
            SelectScreenResolution(model.Game.Resolution);
            m_fullScreenToggle.isOn = model.Game.FullScreen;
            m_playerNameInput.text = model.Game.PlayerName;
            m_musicVolume.value = model.Game.MusicVolume;
            m_soundVolume.value = model.Game.SoundVolume;
        }

        protected override void UpdateGameSettingsModel()
        {
            Model.Game.FullScreen = m_fullScreenToggle.isOn;
            Model.Game.Resolution = m_screenResDropdown.options[m_screenResDropdown.value].text;
            Model.Game.PlayerName = m_playerNameInput.text;
            Model.Game.SoundVolume = m_soundVolume.value;
            Model.Game.MusicVolume = m_musicVolume.value;
        }

        public void Save()
        {
            m_settingsRepo.SetSettingsModel(this.Model);
        }
    }
}