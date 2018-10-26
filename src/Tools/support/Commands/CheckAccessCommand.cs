using System.ComponentModel;
using System.Threading.Tasks;
using NConsole;
using NLog;

namespace support
{
    [Description("Health check of Survey Solutions services.")]
    public class CheckAccessCommand : ConfigurationDependentCommand, IConsoleCommand
    {
        private readonly INetworkService _networkService;
        private readonly IDatabaseService databaseService;
        private readonly ILogger _logger;

        public CheckAccessCommand(INetworkService networkService, 
            IDatabaseService databaseService,
            IConfigurationManagerSettings configurationManagerSettings, ILogger logger) : base(configurationManagerSettings)
        {
            _networkService = networkService;
            this.databaseService = databaseService;
            _logger = logger;
        }

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
            if (!ReadConfigurationFile(host))
                return null;

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
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                host.WriteMessage("Permissions to database: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    databaseService.HasPermissionsAsync(ConnectionString));
            }
            else
                host.WriteMessage("Connection string not found");
        }

        private async Task CheckConnectionToDatabaseAsync(IConsoleHost host)
        {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                host.WriteMessage("Connection to database: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    databaseService.HasConnectionAsync(ConnectionString));
            }
            else
                host.WriteMessage("Connection string not found");
        }

        private async Task CheckConnectionToDesignerAsync(IConsoleHost host)
        {
            if (!string.IsNullOrEmpty(DesignerUrl))
            {
                host.WriteMessage("Connection to the Survey Solutions website: ");
                await host.TryExecuteActionWithAnimationAsync(_logger,
                    _networkService.IsHostReachableAsync(DesignerUrl));
            }
            else
                host.WriteLine("Url to Survey Solutions Website not found.");
        }
    }
}
