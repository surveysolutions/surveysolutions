using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using NConfig;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Web.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly Regex ConnectionStringPasswordRegex = new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected virtual List<string> settingsToSkip => new List<string> { "Headquarters.AccessToken" };

        public virtual IEnumerable<ApplicationSetting> GetSettings()
        {
            foreach (string appSettingKey in AppSettings.AllKeys.Except(settingsToSkip.Contains))
            {
                yield return new ApplicationSetting
                {
                    Name = appSettingKey,
                    Value = AppSettings[appSettingKey]
                };
            }

            for (int connectionStringIndex = 0; connectionStringIndex < ConnectionStrings.Count; connectionStringIndex++)
            {
                var connectionString = ConnectionStrings[connectionStringIndex];
                yield return new ApplicationSetting
                {
                    Name = $"connectionStrings\\{connectionString.Name}",
                    Value = RemovePasswordFromConnectionString(connectionString.ConnectionString),
                };
            }

            var serviceModelClientSection = GetSection<ClientSection>("system.serviceModel/client");
            for (int i = 0; i < serviceModelClientSection.Endpoints.Count; i++)
            {
                yield return new ApplicationSetting
                {
                    Name = serviceModelClientSection.Endpoints[i].Name,
                    Value = serviceModelClientSection.Endpoints[i].Address.ToString()
                };
            }
        }

        public TSection TryGetSection<TSection>(string name) where TSection : ConfigurationSection
            => NConfigurator.Default.GetSection<TSection>(name);

        public TSection GetSection<TSection>(string name) where TSection : ConfigurationSection
        {
            var result = NConfigurator.Default.GetSection<TSection>(name);

            if (result == null)
            {
                throw new ArgumentException($"Cannot find section with name {name}. " +
                    $"If this is custom section that is overriden in another configuration, please make sure that <configSection> contains definition " +
                    $"of custom section", nameof(name));
            }

            return result;
        }

        public NameValueCollection AppSettings => WebConfigurationManager.AppSettings;

        public ConnectionStringSettingsCollection ConnectionStrings => WebConfigurationManager.ConnectionStrings;

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