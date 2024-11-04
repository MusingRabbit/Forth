namespace Assets.Scripts.UI.Models
{
    public class SettingsModel
    {
        public GameSettingsModel Game { get; set; }
        public SessionSettingsModel Session { get; set; }

        public SettingsModel()
        {
            this.Game = new GameSettingsModel();
            this.Session = new SessionSettingsModel();
        }
    }
}
