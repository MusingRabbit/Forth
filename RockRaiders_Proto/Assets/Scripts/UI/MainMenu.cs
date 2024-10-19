using Assets.Scripts;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : GameMenu
{
    public MainMenu()
    {
        
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked.");
        Application.Quit();
    }

    protected override void UpdateControls(SettingsModel model)
    {
    }

    protected override void UpdateGameSettingsModel()
    {
    }
}
