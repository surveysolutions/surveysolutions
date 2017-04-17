using System;
using System.ComponentModel;
using System.Configuration;
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
        [Switch(LongName = "all")]
        public bool All { get; set; } = false;

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
            this.PathToHeadquarters = this.PathToHeadquarters.TrimEnd('\\');

            _configurationManagerSettings.SetPhysicalPathToWebsite(this.PathToHeadquarters);

            var dbConnectionString = ConfigurationManager.ConnectionStrings["Postgres"]?.ConnectionString;
            var designerUrl = ConfigurationManager.AppSettings["DesignerAddress"];

            if (string.IsNullOrEmpty(dbConnectionString) || string.IsNullOrEmpty(designerUrl))
                host.WriteLine("Headquarters website settings not found. " +
                               "Please, ensure that you enter correct path to Headquarters website");
            else
            {
                if (this.All || this.CheckDesignerWebsite)
                {
                    host.WriteMessage("Connection to the Survey Solutions website: ");
                    await host.TryExecuteActionWithAnimationAsync(_logger, _networkService.IsHostReachableAsync(designerUrl));
                }

                if (this.All || this.CheckDbConnection)
                {
                    host.WriteMessage("Connection to database: ");
                    await host.TryExecuteActionWithAnimationAsync(_logger, _databaseSevice.HasConnectionAsync(dbConnectionString));
                }

                if (this.All || this.CheckDbPermissions)
                {
                    host.WriteMessage("Permissions to database: ");
                    await host.TryExecuteActionWithAnimationAsync(_logger, _databaseSevice.HasPermissionsAsync(dbConnectionString));
                }
            }

            return null;
        }
    }
}