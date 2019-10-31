using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using NConsole;

namespace support
{
    public class ConfigurationDependentCommand
    {
        private readonly IConfigurationManagerSettings configurationManagerSettings;

        private string connectionString;
        private string designerUrl;
        private string appDataDirectory;
        private string exportServiceUrl;
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

        protected internal string AppDataDirectory
        {
            get
            {
                ThrowIfSettingsWereNotInitialized();
                return appDataDirectory;
            }
            set => appDataDirectory = value;
        }

        protected internal string ExportServiceUrl
        {
            get
            {
                ThrowIfSettingsWereNotInitialized();
                return exportServiceUrl;
            }
            set => exportServiceUrl = value;
        }

        public ConfigurationDependentCommand(IConfigurationManagerSettings configurationManagerSettings)
        {
            this.configurationManagerSettings = configurationManagerSettings;
        }

        [Description("Physical path to Headquarters website.")]
        [Argument(Name = "path", IsRequired = false)]
        public string PathToHeadquarters { get; set; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\");

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
            var dataStorePath = ConfigurationManager.AppSettings["DataStorePath"];
            ExportServiceUrl = ConfigurationManager.AppSettings["Export.ServiceUrl"];


            if (dataStorePath.StartsWith("~/") || dataStorePath.StartsWith(@"~\"))
            {
                dataStorePath = dataStorePath.Replace(@"~/", "./");
                dataStorePath = dataStorePath.Replace(@"~\", "./");
                AppDataDirectory = Path.GetFullPath( Path.Combine(PathToHq, dataStorePath.Replace("/","\\")));
            }
            else
            {
                AppDataDirectory = dataStorePath;
            }

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
