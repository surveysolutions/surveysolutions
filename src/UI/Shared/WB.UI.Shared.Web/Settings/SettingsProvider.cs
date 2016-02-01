using System.Collections.Generic;
using System.ServiceModel.Configuration;
using System.Web.Configuration;

namespace WB.UI.Shared.Web.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        protected virtual List<string> settingsToSkip
        {
            get
            {
                return new List<string> { "Headquarters.AccessToken", "EventStore.Password" };
            }
        }

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

            for (int connectionStringIndex = 0; connectionStringIndex < WebConfigurationManager.ConnectionStrings.Count; connectionStringIndex++)
            {
                var connectionString = WebConfigurationManager.ConnectionStrings[connectionStringIndex];
                yield return new ApplicationSetting
                {
                    Name = $"connectionStrings\\{connectionString.Name}",
                    Value = connectionString.ConnectionString,
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
        public ApplicationSetting() { }
        public ApplicationSetting(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}