using System.Collections.Specialized;

namespace WB.UI.Shared.Web.Configuration
{
    public class ConfigurationManager : IConfigurationManager
    {
        public ConfigurationManager(NameValueCollection appSettings, NameValueCollection membershipSettings)
        {
            this.MembershipSettings = membershipSettings;
            this.AppSettings = appSettings;
        }

        public NameValueCollection AppSettings { get; private set; }
        public NameValueCollection MembershipSettings { get; private set; }
    }
}
