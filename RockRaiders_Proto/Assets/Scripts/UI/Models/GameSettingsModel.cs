namespace Assets.Scripts
{
    public class GameSettingsModel
    {
        public string PlayerName { get; set; }
        public string Resolution { get; set; }
        public bool FullScreen { get; set; }

        public GameSettingsModel()
        {
            this.Resolution = "800x600@59.94";
            this.PlayerName = "Player";
            this.FullScreen = false;
        }
    }
}
