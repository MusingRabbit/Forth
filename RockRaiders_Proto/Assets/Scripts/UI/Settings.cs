using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Settings : GameMenu
{
    [SerializeField]
    private TMP_Dropdown m_screenResDropdown;

    [SerializeField]
    private Toggle m_fullScreenToggle;

    [SerializeField]
    private TMP_InputField m_playerNameInput;


    // Start is called before the first frame update
    protected override void Start()
    {
        this.PopulateScreenResolutionsDropDown();
        base.Start();

        if (this.Model.GameSettings.Resolution == string.Empty)
        {
            m_screenResDropdown.value = -1;
            m_screenResDropdown.value = 0;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        var oldResolution = this.Model.GameSettings.Resolution;
        var oldFullScreenMode = this.Model.GameSettings.FullScreen;

        base.Update();

        if (oldResolution != this.Model.GameSettings.Resolution)
        {
            this.SelectScreenResolution(this.Model.GameSettings.Resolution);
        }
    }

    public void FullScreenToggle()
    {
        this.Model.GameSettings.FullScreen = !this.Model.GameSettings.FullScreen;
        this.SetScreenResolution(this.Model.GameSettings.Resolution, this.Model.GameSettings.FullScreen);
    }

    private void PopulateScreenResolutionsDropDown()
    {
        m_screenResDropdown.options.Clear();

        foreach (var res in Screen.resolutions)
        {
            m_screenResDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width}x{res.height}@{res.refreshRateRatio}"));
        }
    }

    private void SetScreenResolution(string resolution, bool fullScreen = false)
    {
        var tokens = resolution.Split('x', '@');
        var width = int.Parse (tokens[0]);
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
            this.SetScreenResolution(resolution, this.Model.GameSettings.FullScreen);
        }
        else
        {
            m_screenResDropdown.value = val;
            m_screenResDropdown.value = 0;
        }
        

    }

    protected override void UpdateControls(SettingsModel model)
    {
        this.SelectScreenResolution(model.GameSettings.Resolution);
        m_fullScreenToggle.isOn = model.GameSettings.FullScreen;
        m_playerNameInput.text = model.GameSettings.PlayerName;
    }

    protected override void UpdateGameSettingsModel()
    {
        this.Model.GameSettings.FullScreen = m_fullScreenToggle.isOn;
        this.Model.GameSettings.Resolution = m_screenResDropdown.options[m_screenResDropdown.value].text;
        this.Model.GameSettings.PlayerName = m_playerNameInput.text;
    }
}
