using System.Configuration;

namespace WB.UI.Shared.Web.Configuration
{
    public class ExternalStoragesConfigSection : ConfigurationSection
    {
        private const string oauth2 = "oauth2";

        [ConfigurationProperty(oauth2)]
        public OAuth2ConfigurationElement OAuth2
        {
            get => (OAuth2ConfigurationElement)base[oauth2];
            set => base[oauth2] = value;
        }
    }
    public class OAuth2ConfigurationElement : ConfigurationElement
    {
        private const string dropbox = "dropbox";
        private const string onedrive = "onedrive";
        private const string googledrive = "googledrive";

        private const string redirectUri = "redirectUri";
        private const string responseType = "responseType";

        [ConfigurationProperty(redirectUri, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string RedirectUri
        {
            get => (string)base[redirectUri];
            set => base[redirectUri] = value;
        }

        [ConfigurationProperty(responseType, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ResponseType
        {
            get => (string)base[responseType];
            set => base[responseType] = value;
        }

        [ConfigurationProperty(dropbox)]
        public ExternalStorageConfigurationElement Dropbox
        {
            get => (ExternalStorageConfigurationElement)base[dropbox];
            set => base[dropbox] = value;
        }

        [ConfigurationProperty(onedrive)]
        public ExternalStorageConfigurationElement OneDrive
        {
            get => (ExternalStorageConfigurationElement)base[onedrive];
            set => base[onedrive] = value;
        }

        [ConfigurationProperty(googledrive)]
        public ExternalStorageConfigurationElement GoogleDrive
        {
            get => (ExternalStorageConfigurationElement)base[googledrive];
            set => base[googledrive] = value;
        }
    }

    public class ExternalStorageConfigurationElement : ConfigurationElement
    {
        private const string clientId = "clientId";
        private const string authorizationUri = "authorizationUri";
        private const string scope = "scope";

        [ConfigurationProperty(clientId, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ClientId
        {
            get => (string)base[clientId];
            set => base[clientId] = value;
        }

        [ConfigurationProperty(authorizationUri, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string AuthorizationUri
        {
            get => (string)base[authorizationUri];
            set => base[authorizationUri] = value;
        }

        [ConfigurationProperty(scope, DefaultValue = "", IsKey = true, IsRequired = false)]
        public string Scope
        {
            get => (string)base[scope];
            set => base[scope] = value;
        }
    }
}
