using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using NConsole;

namespace support
{
    public class ConfigurationDependentCommand
    {
        private readonly IConfigurationManagerSettings configurationManagerSettings;

        private string connectionString;
        private string designerUrl;
        protected internal string PathToHq;

        protected internal string ConnectionString
        {
            get
            {
                ThrowIfSettingsWereNotInitialized();
                return connectionString;
            }
            private set => connectionString = value;
        }

        protected internal string DesignerUrl
        {
            get
            {
                ThrowIfSettingsWereNotInitialized();
                return designerUrl;
            }
            set => designerUrl = value;
        }

        public ConfigurationDependentCommand(IConfigurationManagerSettings configurationManagerSettings)
        {
            this.configurationManagerSettings = configurationManagerSettings;
        }

        [Description("Physical path to Headquarters website.")]
        [Argument(Name = "path")]
        public string PathToHeadquarters { get; set; }

        protected bool ReadConfigurationFile(IConsoleHost host)
        {
            PathToHq = this.PathToHeadquarters.Trim('"').TrimEnd('\\');
            if (!Directory.Exists(PathToHq))
            {
                host.WriteLine("Headquarters website settings not found. " +
                               "Please, ensure that you enter correct path to Headquarters website");
                return false;
            }
            
            configurationManagerSettings.SetPhysicalPathToWebsite(PathToHq);

            ConnectionString = ConfigurationManager.ConnectionStrings["Postgres"]?.ConnectionString;
            DesignerUrl = ConfigurationManager.AppSettings["DesignerAddress"];

            return true;
        }

        private void ThrowIfSettingsWereNotInitialized()
        {
            if (!configurationManagerSettings.IsInitialized)
                throw new InvalidOperationException(
                    "Initialize configuration manager settings before accessing to configuration parameters. Call ReadConfigurationFile() in RunAsync");
        }
    }
}
