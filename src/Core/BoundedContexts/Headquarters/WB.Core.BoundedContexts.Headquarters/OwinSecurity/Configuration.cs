using System.Data.Entity;
using System.Data.Entity.Migrations;
using WB.Core.BoundedContexts.Headquarters.Migrator;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal sealed class Configuration : DbMigrationsConfiguration<HQIdentityDbContext>
    {
        public const string SchemaName = "users";
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetHistoryContextFactory("Npgsql", (connection, defaultSchema) => new SchemaBasedHistoryContext(connection, defaultSchema, SchemaName));
        }

        public DbSet<HqUserProfile> UserProfiles { get; set; }

        private static readonly object onMigration = new object();

        protected override void Seed(HQIdentityDbContext context)
        {
            //  This method will be called after migrating to the latest version. And after each build...

            lock (onMigration)
            {
                var migrator = new DataMigrator<HQIdentityDbContext>();
                migrator.MigrateToLatest(context);
            }
        }
    }
}
