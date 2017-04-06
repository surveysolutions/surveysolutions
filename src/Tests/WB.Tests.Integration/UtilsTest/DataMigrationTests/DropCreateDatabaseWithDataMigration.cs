using System.Data.Entity;
using WB.Core.BoundedContexts.Headquarters.Migrator;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    internal sealed class DropCreateDatabaseWithDataMigration<TDbContext> : DropCreateDatabaseAlways<TDbContext> where TDbContext : DbContext, IDataMigrationContext
    {
        private static readonly object onMigration = new object();

        protected override void Seed(TDbContext context)
        {
            lock (onMigration)
            {
                try
                {
                    var migrator = new DataMigrator<TDbContext>();
                    migrator.MigrateToLatest(context);
                    migrator.MigrateToLatest(context); // to ensure that migrations are run only once
                }
                catch (System.Exception)
                {
                    // om om om, for tests purpose we will not throw 
                }
            }
        }
    }
}