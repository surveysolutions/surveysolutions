using System.Collections.Generic;
using System.Reflection;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresPlainStorageSettings
    {
        public PostgresPlainStorageSettings()
        {
            this.MappingAssemblies = new List<Assembly>();
        }

        public string ConnectionString { get; set; }

        public List<Assembly> MappingAssemblies { get; set; }
    }
}