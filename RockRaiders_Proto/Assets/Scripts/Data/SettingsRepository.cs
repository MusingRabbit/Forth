using Assets.Scripts.Services;
using Assets.Scripts.UI.Models;
using RockRaiders.Util.Helpers;
using System;
using System.IO;

namespace Assets.Scripts.Data
{
    public class SettingsRepository
    {
        private string m_configFilePath = "gameSettings.xml";

        public event EventHandler<EventArgs> OnSettingsModelSet;

        public SettingsRepository()
        {
        }

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

        public void SetSettingsModel(SettingsModel settings)
        {
            var doc = XMLHelper.SerializeToXMLDocument(settings);
            File.WriteAllText(m_configFilePath, doc.InnerXml);
            this.OnSettingsModelSet?.Invoke(this, EventArgs.Empty);
        }
    }
}
