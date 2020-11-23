#nullable enable
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspacesService
    {
        public Task Generate(string name, DbUpgradeSettings upgradeSettings);
    }
    
    class WorkspacesService : IWorkspacesService
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        private readonly ILoggerProvider loggerProvider;

        public WorkspacesService(UnitOfWorkConnectionSettings connectionSettings,
            ILoggerProvider loggerProvider)
        {
            this.connectionSettings = connectionSettings;
            this.loggerProvider = loggerProvider;
        }

        public Task Generate(string name, DbUpgradeSettings upgradeSettings)
        {
            string schemaName = "ws_" + name;
            DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                schemaName);
            return Task.Run(() => DbMigrationsRunner.MigrateToLatest(connectionSettings.ConnectionString,
                schemaName,
                upgradeSettings, loggerProvider));
        }
    }
}
