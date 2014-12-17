using System.Collections.Generic;
using System.ServiceModel.Configuration;
using System.Web.Configuration;

namespace WB.UI.Shared.Web.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private static List<string> settingsToSkip = new List<string>()
        {
            "Headquarters.AccessToken",
            "EventStore.Password"
        };

        public virtual IEnumerable<ApplicationSetting> GetSettings()
        {
            foreach (string appSettingKey in WebConfigurationManager.AppSettings.AllKeys)
            {
                if (settingsToSkip.Contains(appSettingKey)) continue;
                yield return new ApplicationSetting

                {
                    Name = appSettingKey,
                    Value = WebConfigurationManager.AppSettings[appSettingKey]
                };
            }

            var section = (ClientSection)WebConfigurationManager.GetSection("system.serviceModel/client");
            for (int i = 0; i < section.Endpoints.Count; i++)
            {
                yield return new ApplicationSetting
                {
                    Name = section.Endpoints[i].Name,
                    Value = section.Endpoints[i].Address.ToString()
                };
            }
        }
    }

    public class ApplicationSetting 
    {
        public ApplicationSetting() {}
        public ApplicationSetting(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}