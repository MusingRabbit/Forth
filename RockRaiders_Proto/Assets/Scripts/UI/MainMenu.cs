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
}