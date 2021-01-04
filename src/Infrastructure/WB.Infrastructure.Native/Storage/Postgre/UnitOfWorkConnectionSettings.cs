using System.Collections.Generic;
using System.Reflection;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class UnitOfWorkConnectionSettings
    {
        public string ConnectionString { get; set; }

        public string PlainStorageSchemaName => "plainstore";
        public string ReadSideSchemaName => "readside";

        public IList<Assembly> PlainMappingAssemblies { get; set; }
        public IList<Assembly> ReadSideMappingAssemblies { get; set; }
        public DbUpgradeSettings ReadSideUpgradeSettings { get; set; }
        public DbUpgradeSettings PlainStoreUpgradeSettings { get; set; }
        public DbUpgradeSettings LogsUpgradeSettings { get; set; }
        public string LogsSchemaName => "logs";
        public DbUpgradeSettings UsersUpgradeSettings { get; set; }
        
        public DbUpgradeSettings MigrateToPrimaryWorkspace { get; set; }
        public string PrimaryWorkspaceSchemaName => "ws_primary";
        
        public string UsersSchemaName => "users";
        public DbUpgradeSettings EventStoreUpgradeSettings { get; set; }
        public string EventsSchemaName => "events";

        public DbUpgradeSettings WorkspacesMigrationSettings { get; set; }

        public string WorkspacesSchemaName => "workspaces";
        
        public DbUpgradeSettings SingleWorkspaceUpgradeSettings { get; set; }
    }
}
