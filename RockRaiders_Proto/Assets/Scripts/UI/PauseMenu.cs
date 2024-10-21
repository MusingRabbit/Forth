using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : GameMenu
{
    public PauseMenu()
    {
        
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public void NotifyPauseMenuClosed()
    {
        this.GameManager.NotifyPauseMenuClosed();
    }

    public void Quit()
    {
        this.GameManager.QuitGame();
    }

    protected override void UpdateControls(SettingsModel model)
    {
    }

    protected override void UpdateGameSettingsModel()
    {
    }

}
