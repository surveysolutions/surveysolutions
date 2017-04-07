
using System.Data.Entity;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Migrator;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    [TestFixture]
    public class When_migration_success_DataMigratorTests : nunit_ef_postgredb_test_specification
    {
        [Test]
        public async Task Should_run_two_data_migrations()
        {
            var ctx = new TestOkDbContext();

            var migrations = await ctx.DataMigrations.ToListAsync();

            Assert.That(migrations, Has.Count.EqualTo(2));
            Assert.That(migrations[0], Has.Property(nameof(DataMigrationInfo.Id)).EqualTo("20170407124702"));
            Assert.That(migrations[0], Has.Property(nameof(DataMigrationInfo.Name)).EqualTo(nameof(TestOkEntityMigrationTest1)));

            Assert.That(migrations[1], Has.Property(nameof(DataMigrationInfo.Id)).EqualTo("20170407125222"));
            Assert.That(migrations[1], Has.Property(nameof(DataMigrationInfo.Name)).EqualTo(nameof(TestOkEntityMigrationTest2)));
        }

        [Test]
        public void Should_run_first_migration_and_fill_data()
        {
            var ctx = new TestOkDbContext();

            var entity = ctx.TestEntities.Find(1);
            Assert.That(entity.Id, Is.EqualTo(1));
            Assert.That(entity.Value, Is.EqualTo("Test migration 1"));
        }

        [Test]
        public void Should_run_second_migration_and_fill_data()
        {
            var ctx = new TestOkDbContext();

            var entity = ctx.TestEntities.Find(2);
            Assert.That(entity.Id, Is.EqualTo(2));
            Assert.That(entity.Value, Is.EqualTo("Test migration 2"));
        }

        internal sealed class TestOkDbContext : TestDbContext
        {
            public TestOkDbContext() : base()
            {
                Database.SetInitializer(new MigrateDataInitializer<TestOkDbContext>());
            }
        }

        internal class TestOkEntityMigrationTest1 : DataMigration<TestOkDbContext>
        {
            public override string Id => "20170407124702";

            public override void Up(TestOkDbContext context)
            {
                context.TestEntities.Add(new TestEntity()
                {
                    Id = 1,
                    Value = "Test migration 1"
                });

                context.SaveChanges();
            }
        }

        internal class TestOkEntityMigrationTest2 : DataMigration<TestOkDbContext>
        {
            public override string Id => "20170407125222";

            public override void Up(TestOkDbContext context)
            {
                context.TestEntities.Add(new TestEntity()
                {
                    Id = 2,
                    Value = "Test migration 2"
                });

                context.SaveChanges();
            }
        }
    }
}