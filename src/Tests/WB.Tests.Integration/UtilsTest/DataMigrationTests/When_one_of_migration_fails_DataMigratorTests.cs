using System;
using System.Data.Entity;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Migrator;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    [TestFixture]
    public class When_one_of_migration_fails_DataMigratorTests : nunit_ef_postgredb_test_specification
    {
        [Test]
        public async Task Should_not_store_any_migrations()
        {
            var ctx = new TestFailingDbContext();

            var migrations = await ctx.DataMigrations.ToListAsync();

            Assert.That(migrations, Has.Count.EqualTo(0));
        }

        [Test]
        public void Should_not_store_data_from_first_migration()
        {
            var ctx = new TestFailingDbContext();

            var entity = ctx.TestEntities.Find(1);
            Assert.IsNull(entity);
        }

        internal sealed class TestFailingDbContext : TestDbContext
        {
            public TestFailingDbContext() 
            {
                Database.SetInitializer(new MigrateDataInitializer<TestFailingDbContext>());
            }
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