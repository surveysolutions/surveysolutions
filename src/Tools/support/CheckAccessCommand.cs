using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NConsole;

namespace support
{
    [Description("Health check of Survey Solutions services.")]
    public class CheckAccessCommand : IConsoleCommand
    {
        private readonly INetworkService _networkService;
        private readonly IDatabaseSevice _databaseSevice;
        private readonly IConfigurationManagerSettings _configurationManagerSettings;

        public CheckAccessCommand(INetworkService networkService, IDatabaseSevice databaseSevice,
            IConfigurationManagerSettings configurationManagerSettings)
        {
            _networkService = networkService;
            _databaseSevice = databaseSevice;
            _configurationManagerSettings = configurationManagerSettings;
        }

        [Description("Physical path to Headquarters website.")]
        [Argument(Name = "path")]
        public string PathToHeadquarters { get; set; }

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
                if (!this.CheckDesignerWebsite && !this.CheckDbConnection && !this.CheckDbPermissions)
                    host.WriteLine("No health checks selected");

                if (this.CheckDesignerWebsite)
                {
                    host.WriteMessage("Connection to the Survey Solutions website: ");
                    SpinAnimation.Start();
                    var isWebsiteReachable =
                        await _networkService.IsHostReachableAsync(designerUrl).ConfigureAwait(false);
                    SpinAnimation.Stop();

                    if (isWebsiteReachable)
                        host.WriteOk();
                    else
                        host.WriteFailed();
                }

                if (this.CheckDbConnection)
                {
                    host.WriteMessage("Connection to database: ");

                    SpinAnimation.Start();
                    var hasConnectionToDatabase = await _databaseSevice.HasConnectionAsync(dbConnectionString).ConfigureAwait(false);
                    SpinAnimation.Stop();

                    if (hasConnectionToDatabase)
                        host.WriteOk();
                    else
                        host.WriteFailed();
                }

                if (this.CheckDbPermissions)
                {
                    host.WriteMessage("Permissions to database: ");

                    SpinAnimation.Start();
                    var hasPermissionsToDatabase = (await _databaseSevice.HasPermissionsAsync(dbConnectionString).ConfigureAwait(false));
                    SpinAnimation.Stop();

                    if (hasPermissionsToDatabase)
                        host.WriteOk();
                    else
                        host.WriteFailed();

                }
            }

            return null;
        }
    }
}