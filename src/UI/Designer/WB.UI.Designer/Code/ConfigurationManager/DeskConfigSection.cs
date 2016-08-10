using System.Configuration;

namespace WB.UI.Designer.Code.ConfigurationManager
{
    public class DeskConfigSection : ConfigurationSection
    {
        private const string SITEKEY = "siteKey";
        private const string MULTIPASSKEY = "multipassKey";
        private const string RETURNURLFORMAT = "returnUrlFormat";

        [ConfigurationProperty(SITEKEY, IsRequired = true)]
        public DeskConfigurationElement SiteKey
        {
            get { return ((DeskConfigurationElement)(base[SITEKEY])); }
            set { base[SITEKEY] = value; }
        }

        [ConfigurationProperty(MULTIPASSKEY, IsRequired = true)]
        public DeskConfigurationElement MultipassKey
        {
            get { return ((DeskConfigurationElement)(base[MULTIPASSKEY])); }
            set { base[MULTIPASSKEY] = value; }
        }

        [ConfigurationProperty(RETURNURLFORMAT, IsRequired = true)]
        public DeskConfigurationElement ReturnUrlFormat
        {
            get { return ((DeskConfigurationElement)(base[RETURNURLFORMAT])); }
            set { base[RETURNURLFORMAT] = value; }
        }

        public class DeskConfigurationElement : ConfigurationElement
        {
            private const string VALUE = "value";

            [ConfigurationProperty(VALUE, IsRequired = true)]
            public string Value
            {
                get { return (string)(base[VALUE]); }
                set { base[VALUE] = value; }
            }
        }
    }
}