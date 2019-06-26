using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NConsole;
using NLog;
using support.Services;

namespace support
{
    [Description("Health check of Survey Solutions services.")]
    public class CheckHealthCommand : ConfigurationDependentCommand, IConsoleCommand
    {
        private readonly INetworkService networkService;
        private readonly IDatabaseService databaseService;
        private readonly ISystemService systemService;

        private readonly ILogger logger;

        public CheckHealthCommand(INetworkService networkService, 
            IDatabaseService databaseService,
            IConfigurationManagerSettings configurationManagerSettings,
            ISystemService systemService,
            ILogger logger) : base(configurationManagerSettings)
        {
            this.networkService = networkService;
            this.databaseService = databaseService;
            this.logger = logger;
            this.systemService = systemService;
        }

        [Description("Check access to Survey Solutions website, connection and permissions to Headquarters database.")]
        [NConsole.Switch(LongName = "all", ShortName = "all")]
        public bool All { get; set; } = true;

        [Description("Check access to Survey Solutions website.")]
        [NConsole.Switch(ShortName = "ss", LongName = "survey-solutions")]
        public bool CheckDesignerWebsite { get; set; } = false;

        [Description("Check access to Headquarters database.")]
        [NConsole.Switch(ShortName = "dbc", LongName = "db-connection")]
        public bool CheckDbConnection { get; set; } = false;

        [Description("Check permissions to Headquarters database.")]
        [NConsole.Switch(ShortName = "dbp", LongName = "db-permissions")]
        public bool CheckDbPermissions { get; set; } = false;

        [Description("Check Export service.")]
        [NConsole.Switch(ShortName = "es", LongName = "export-service")]
        public bool CheckExportService { get; set; } = false;

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            logger.Info("Health check command");

            if (!ReadConfigurationFile(host))
                return null;

            var shouldCheckDesignerWebsite = this.All || this.CheckDesignerWebsite;
            var shouldCheckDbConnection = this.All || this.CheckDbConnection;
            var shouldCheckDbPermissions = this.All || this.CheckDbPermissions;
            var shouldCheckExportService = this.All || this.CheckExportService;

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

            if (shouldCheckExportService)
                await CheckExportServiceAsync(host);

            return null;
        }
        
        private async Task CheckPermissionsToDatabaseAsync(IConsoleHost host)
        {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                logger.Info("Checking permissions to DB");
                host.WriteMessage("Permissions to database: ");
                await host.TryExecuteActionWithAnimationAsync(logger,
                    databaseService.HasPermissionsAsync(ConnectionString),
                    "User has no permissions to access database. Please ensure that user has permissions.");
            }
            else
                host.WriteMessage("Connection string not found");
        }

        private async Task CheckConnectionToDatabaseAsync(IConsoleHost host)
        {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                logger.Info("Checking connection to DB");
                host.WriteMessage("Connection to database: ");
                await host.TryExecuteActionWithAnimationAsync(logger,
                    databaseService.HasConnectionAsync(ConnectionString),
                    "Database was not available. Please check that server can be reached and DB exists.");
            }
            else
                host.WriteMessage("Connection string not found");
        }

        private async Task CheckConnectionToDesignerAsync(IConsoleHost host)
        {
            if (!string.IsNullOrEmpty(DesignerUrl))
            {
                logger.Info("Checking connection to Survey Solutions");
                host.WriteMessage("Connection to the Survey Solutions website: ");
                await host.TryExecuteActionWithAnimationAsync(logger,
                    networkService.IsHostReachableAsync(DesignerUrl),
                    $"Survey Solutions Designer is not reachable. Please check that site {DesignerUrl} can be reached from the server.");
            }
            else
                host.WriteLine("Url to Survey Solutions Website not found.");
        }

        private async Task CheckExportServiceAsync(IConsoleHost host)
        {
            if (!string.IsNullOrEmpty(ExportServiceUrl))
            {
                logger.Info("Checking connection to Export Service");
                host.WriteMessage("Connection to the Export Service: ");
                await host.TryExecuteActionWithAnimationAsync(logger,
                    networkService.IsHostReachableAsync(ExportServiceUrl),
                    $"Export service access point {ExportServiceUrl} is not reachable. Please check that it's running and healthy.");

                var processName = "WB.Services.Export.Host";

                host.WriteMessage("Checking running Export Service: ");
                await host.TryExecuteActionWithAnimationAsync(logger,
                    this.systemService.IsProcessRunning(processName),
                    $"Export service process {processName} was not found. Please check that service can be started.");
            }
            else
                host.WriteLine("Export Service Access point was not found.");

        }
    }
}
