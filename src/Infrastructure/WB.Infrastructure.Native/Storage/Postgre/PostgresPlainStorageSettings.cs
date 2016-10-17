using System.Collections.Generic;
using System.Reflection;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresPlainStorageSettings
    {
        public PostgresPlainStorageSettings()
        {
            this.MappingAssemblies = new List<Assembly>();
        }

        public string ConnectionString { get; set; }

        public DbUpgradeSettings DbUpgradeSettings { get; set; }

        public List<Assembly> MappingAssemblies { get; set; }
        public string SchemaName { get; set; }
    }
}