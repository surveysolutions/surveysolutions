using System.Data.Entity.Migrations;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DbConfiguration : DbMigrationsConfiguration<HQPlainStorageDbContext>
    {
        public const string SchemaName = "plainstore";
        public DbConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
            this.SetHistoryContextFactory("Npgsql",
                (connection, defaultSchema) => new SchemaBasedHistoryContext(connection, defaultSchema, SchemaName));
        }
    }
}