using NConfig;

namespace support
{
    public class ConfigurationManagerSettings : IConfigurationManagerSettings
    {
        private bool isInitialized;
        public bool IsInitialized => isInitialized;

        public void SetPhysicalPathToWebsite(string path)
        {
            if (this.isInitialized) return;

            NConfigurator.UsingFiles($@"{path}\Configuration\Headquarters.Web.config",
                $@"{path}\Web.config").SetAsSystemDefault();
            this.isInitialized = true;
        }
    }
}
