using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using NConsole;
using NLog;

namespace support
{
    [Description("Health check of Survey Solutions services.")]
    public class CheckAccessCommand : IConsoleCommand
    {
        private readonly INetworkService _networkService;
        private readonly IDatabaseSevice _databaseSevice;
        private readonly IConfigurationManagerSettings _configurationManagerSettings;
        private readonly ILogger _logger;

        public CheckAccessCommand(INetworkService networkService, IDatabaseSevice databaseSevice,
            IConfigurationManagerSettings configurationManagerSettings, ILogger logger)
        {
            _networkService = networkService;
            _databaseSevice = databaseSevice;
            _configurationManagerSettings = configurationManagerSettings;
            _logger = logger;
        }

        [Description("Physical path to Headquarters website.")]
        [Argument(Name = "path")]
        public string PathToHeadquarters { get; set; }

        [Description("Check access to Survey Solutions website, connection and permissions to Headquarters database.")]
        [Switch(LongName = "all", ShortName = "all")]
        public bool All { get; set; } = true;

        [Description("Check access to Survey Solutions website.")]
        [Switch(ShortName = "ss", LongName = "survey-solutions")]
        public bool CheckDesignerWebsite { get; set; } = false;

        [Description("Check access to Headquarters database.")]
        [Switch(ShortName = "dbc", LongName = "db-connection")]
        public bool CheckDbConnection { get; set; } = false;

        [Description("Check permissions to Headquarters database.")]
        [Switch(ShortName = "dbp", LongName = "db-permissions")]
        public bool CheckDbPermissions { get; set; } = false;

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            var pathToHq = this.PathToHeadquarters.Trim('"').TrimEnd('\\');
            if (!Directory.Exists(pathToHq))
            {
                host.WriteLine("Headquarters website settings not found. " +
                               "Please, ensure that you enter correct path to Headquarters website");
                return null;
            }

            _configurationManagerSettings.SetPhysicalPathToWebsite(pathToHq);


            var shouldCheckDesignerWebsite = this.All || this.CheckDesignerWebsite;
            var shouldCheckDbConnection = this.All || this.CheckDbConnection;
            var shouldCheckDbPermissions = this.All || this.CheckDbPermissions;

            if (!shouldCheckDesignerWebsite && !shouldCheckDbConnection && !shouldCheckDbPermissions)
            {
                host.WriteMessage("No health checks selected. Use [--all] parameter to check all services");
                return null;
            }

            if (shouldCheckDesignerWebsite)
                await CheckConnectionToDesignerAsync(host);

            if (shouldCheckDbConnection)
                await CheckConnectionToDatabaseAsync(host);

            if (shouldCheckDbPermissions)
                await CheckPermissionsToDatabaseAsync(host);

            return null;
        }

        private async Task CheckPermissionsToDatabaseAsync(IConsoleHost host)
        {
            var dbConnectionString = ConfigurationManager.ConnectionStrings["Postgres"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(dbConnectionString))
            {
                host.WriteMessage("Permissions to database: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    _databaseSevice.HasPermissionsAsync(dbConnectionString));
            }
            else
                host.WriteMessage("Connection string not found");

        }

        private async Task CheckConnectionToDatabaseAsync(IConsoleHost host)
        {
            var dbConnectionString = ConfigurationManager.ConnectionStrings["Postgres"]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(dbConnectionString))
            {
                host.WriteMessage("Connection to database: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    _databaseSevice.HasConnectionAsync(dbConnectionString));
            }
            else
                host.WriteMessage("Connection string not found");
        }

        private async Task CheckConnectionToDesignerAsync(IConsoleHost host)
        {
            var designerUrl = ConfigurationManager.AppSettings["DesignerAddress"];
            if (!string.IsNullOrEmpty(designerUrl))
            {
                host.WriteMessage("Connection to the Survey Solutions website: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    _networkService.IsHostReachableAsync(designerUrl));
            }
            else
                host.WriteLine("Url to Survey Solutions Website not found.");
        }
    }
}