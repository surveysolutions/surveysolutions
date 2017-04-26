using NConfig;

namespace support
{
    public class ConfigurationManagerSettings : IConfigurationManagerSettings
    {
        private bool _isInitialized;
        public void SetPhysicalPathToWebsite(string path)
        {
            if (this._isInitialized) return;

            path = path.Trim('"').TrimEnd('\\');

            NConfigurator.UsingFiles($@"{path}\Configuration\Headquarters.Web.config",
                $@"{path}\Web.config").SetAsSystemDefault();
            this._isInitialized = true;
        }
    }
}