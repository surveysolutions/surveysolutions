using System;
using System.Data.Common;
using System.Data.Entity;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Migrator;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    [TestFixture]
    public class When_one_of_migration_fails_DataMigratorTests : with_postgres_db
    {
        [OneTimeSetUp]
        public void Setup() => Context();

        [OneTimeTearDown]
        public void TearDown() => Cleanup();

        [Test]
        public async Task Should_not_store_any_migrations()
        {
            var ctx = new TestFailingDbContext(new NpgsqlConnection(connectionStringBuilder.ConnectionString));

            var migrations = await ctx.DataMigrations.ToListAsync();

            Assert.That(migrations, Has.Count.EqualTo(0));
        }

        [Test]
        public void Should_not_store_data_from_first_migration()
        {
            var ctx = new TestFailingDbContext(new NpgsqlConnection(connectionStringBuilder.ConnectionString));

            var entity = ctx.TestEntities.Find(1);
            Assert.IsNull(entity);
        }

        internal sealed class TestFailingDbContext : DbContext, IDataMigrationContext
        {
            public TestFailingDbContext(DbConnection db) : base(db, true)
            {
                Database.SetInitializer(new DropCreateDatabaseWithDataMigration<TestFailingDbContext>());
            }

            // Here you define your own DbSet's
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<TestEntity>().ToTable("entity", "test");
                modelBuilder.Entity<DataMigrationInfo>().ToTable("dataMigrations", "test");
            }

            public IDbSet<DataMigrationInfo> DataMigrations { get; set; }
            public IDbSet<TestEntity> TestEntities { get; set; }
        }

        internal class TestFailingEntityMigrationTest1 : DataMigration<TestFailingDbContext>
        {
            public override string Id => "20170407014331";

            public override void Up(TestFailingDbContext context)
            {
                context.TestEntities.Add(new TestEntity()
                {
                    Id = 1,
                    Value = "Test migration 1"
                });

                context.SaveChanges();
            }
        }

        internal class TestFailingEntityMigrationTest2 : DataMigration<TestFailingDbContext>
        {
            public override string Id => "20170407014339";

            public override void Up(TestFailingDbContext context)
            {
                throw new Exception();
            }
        }
    }
}