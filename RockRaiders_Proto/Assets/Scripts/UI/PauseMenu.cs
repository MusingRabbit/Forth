using Assets.Scripts.UI;
using Assets.Scripts.UI.Models;

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

    public void Respawn()
    {
        this.GameManager.RespawnLocalPlayer();
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
