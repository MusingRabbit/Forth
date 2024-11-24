using Assets.Scripts.Services;
using Assets.Scripts.UI.Models;
using RockRaiders.Util.Helpers;
using System;
using System.IO;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Settings repository - Reponsible for the storage and retreival of settings data
    /// </summary>
    public class SettingsRepository
    {
        /// <summary>
        /// Config file path - hardcoded
        /// </summary>
        private string m_configFilePath = "gameSettings.xml";

        /// <summary>
        /// Triggered when settings have been saved
        /// </summary>
        public event EventHandler<EventArgs> OnSettingsModelSet;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsRepository()
        {
        }

        /// <summary>
        /// Gets the settings model from the config file path
        /// </summary>
        /// <returns>Settings model <see cref="SettingsModel"/></returns>
        public SettingsModel GetSettingsModel()
        {
            var result = new SettingsModel();

            try
            {
                if (File.Exists(m_configFilePath))
                {
                    var text = File.ReadAllText(m_configFilePath);
                    result = XMLHelper.Deserialise<SettingsModel>(text);
                }
            }

            catch(Exception ex)
            {
                NotificationService.Instance.Error(ex);
            }

            return result;
        }

        /// <summary>
        /// Sets / Saves all settings data
        /// </summary>
        /// <param name="settings">Settings model <see cref="SettingsModel"/></param>
        public void SetSettingsModel(SettingsModel settings)
        {
            var doc = XMLHelper.SerializeToXMLDocument(settings);
            File.WriteAllText(m_configFilePath, doc.InnerXml);
            this.OnSettingsModelSet?.Invoke(this, EventArgs.Empty);
        }
    }
}
