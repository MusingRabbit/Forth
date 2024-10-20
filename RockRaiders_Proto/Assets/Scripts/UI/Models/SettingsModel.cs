namespace Assets.Scripts.UI.Models
{
    public class SettingsModel
    {
        public GameSettingsModel GameSettings { get; set; }
        public MatchSettingsModel MatchSettings { get; set; }

        public SettingsModel()
        {
            this.GameSettings = new GameSettingsModel();
            this.MatchSettings = new MatchSettingsModel();
        }
    }
}
