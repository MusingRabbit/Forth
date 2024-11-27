namespace Assets.Scripts.UI.Models
{
    /// <summary>
    /// Settings model
    /// </summary>
    public class SettingsModel
    {
        /// <summary>
        /// Gets or sets the settings model
        /// </summary>
        public GameSettingsModel Game { get; set; }

        /// <summary>
        /// Gets or sets the session model
        /// </summary>
        public SessionSettingsModel Session { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsModel()
        {
            this.Game = new GameSettingsModel();
            this.Session = new SessionSettingsModel();
        }
    }
}
