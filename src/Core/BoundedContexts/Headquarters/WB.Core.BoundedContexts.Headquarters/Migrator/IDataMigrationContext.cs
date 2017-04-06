using System.Data.Entity;

namespace WB.Core.BoundedContexts.Headquarters.Migrator
{
    public interface IDataMigrationContext
    {
        IDbSet<DataMigrationInfo> DataMigrations { get; set; }
    }
}