using System.Collections.Specialized;

namespace WB.UI.Shared.Web.Configuration
{
    public class ConfigurationManager : IConfigurationManager
    {
        public ConfigurationManager(NameValueCollection appSettings)
        {
            this.AppSettings = appSettings;
        }
        public NameValueCollection AppSettings { get; private set; }
    }
}
