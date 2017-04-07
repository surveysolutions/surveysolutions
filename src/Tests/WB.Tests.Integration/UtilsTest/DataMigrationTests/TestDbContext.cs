using System.Data.Entity;
using System.Data.Entity.Migrations;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Migrator;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    internal class TestDbContext : DbContext, IDataMigrationContext
    {
        public TestDbContext() : base(new NpgsqlConnection(TestContext.CurrentContext.Test.Properties.Get("connection") as string), true)
        {
        }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity>().ToTable("entity", "test");
            modelBuilder.Entity<DataMigrationInfo>().ToTable("dataMigrations", "test");
        }

        public IDbSet<DataMigrationInfo> DataMigrations { get; set; }
        public IDbSet<TestEntity> TestEntities { get; set; }

        internal sealed class MigrateDataInitializer<TContext> : MigrateDatabaseToLatestVersion<TContext, TestMigrationConfiguration<TContext>> where TContext : DbContext, IDataMigrationContext
        {
            // this ctor is required by EF
            public MigrateDataInitializer() : base() { }
        }

        internal sealed class TestMigrationConfiguration<TContext> : DbMigrationsConfiguration<TContext> where TContext : DbContext, IDataMigrationContext
        {
            public const string SchemaName = "test";
            public TestMigrationConfiguration()
            {
                this.AutomaticMigrationsEnabled = true;
                this.AutomaticMigrationDataLossAllowed = true;
                this.SetHistoryContextFactory("Npgsql", (connection, defaultSchema) => new SchemaBasedHistoryContext(connection, defaultSchema, SchemaName));
            }

            public DbSet<HqUserProfile> UserProfiles { get; set; }

            private static readonly object onMigration = new object();

            protected override void Seed(TContext context)
            {
                lock (onMigration)
                {
                    try
                    {
                        var migrator = new DataMigrator<TContext>();
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
}