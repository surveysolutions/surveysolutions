using System.Collections.Generic;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Web.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly Regex ConnectionStringPasswordRegex = new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected virtual List<string> settingsToSkip => new List<string> { "Headquarters.AccessToken", "EventStore.Password" };

        public virtual IEnumerable<ApplicationSetting> GetSettings()
        {
            foreach (string appSettingKey in WebConfigurationManager.AppSettings.AllKeys.Except(settingsToSkip.Contains))
            {
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
                    Value = RemovePasswordFromConnectionString(connectionString.ConnectionString),
                };
            }

            var serviceModelClientSection = (ClientSection)WebConfigurationManager.GetSection("system.serviceModel/client");
            for (int i = 0; i < serviceModelClientSection.Endpoints.Count; i++)
            {
                yield return new ApplicationSetting
                {
                    Name = serviceModelClientSection.Endpoints[i].Name,
                    Value = serviceModelClientSection.Endpoints[i].Address.ToString()
                };
            }
        }

        private static string RemovePasswordFromConnectionString(string connectionString)
            => ConnectionStringPasswordRegex.Replace(connectionString, "Password=*****;");
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