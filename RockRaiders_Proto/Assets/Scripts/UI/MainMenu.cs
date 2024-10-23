using Assets.Scripts.Services;
using Assets.Scripts.UI.Models;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MainMenu : GameMenu
    {
        public MainMenu()
        {

        }

        public void QuitGame()
        {
            NotificationService.Instance.Info("Quit");
            Application.Quit();
        }

        protected override void UpdateControls(SettingsModel model)
        {
        }

        protected override void UpdateGameSettingsModel()
        {
        }
    }
}