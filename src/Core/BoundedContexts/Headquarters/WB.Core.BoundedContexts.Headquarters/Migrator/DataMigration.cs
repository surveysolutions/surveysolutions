using System.Data.Entity;

namespace WB.Core.BoundedContexts.Headquarters.Migrator
{
    public abstract class DataMigration<TDbContext> where TDbContext : DbContext
    {
        // Migration id. Migration sorted by this property. Use timestamps.
        public abstract string Id { get; }

        public abstract void Up(TDbContext context);
    }
}