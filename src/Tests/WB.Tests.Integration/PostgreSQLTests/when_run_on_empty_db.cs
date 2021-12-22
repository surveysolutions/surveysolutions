using System;
using System.Configuration;
using System.Reflection;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.Events;
using WB.Persistence.Headquarters.Migrations.Logs;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.Quartz;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Persistence.Headquarters.Migrations.Users;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class when_run_on_empty_db
    {
        private string connectionString;

        [OneTimeSetUp]
        public void create_empty_database()
        {
            var TestConnectionString = TestsConfigurationManager.ConnectionString;
            var databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = $"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }

            connectionString = connectionStringBuilder.ConnectionString;
        }

        [Test]
        public void then_should_create_db_and_run_all_migrations_successfully()
        {
            TestDelegate runner = () =>
            {
                DatabaseManagement.InitDatabase(connectionString, "plainstore");
                DatabaseManagement.InitDatabase(connectionString, "readside");
                DatabaseManagement.InitDatabase(connectionString, "users");
                DatabaseManagement.InitDatabase(connectionString, "events");
                DatabaseManagement.InitDatabase(connectionString, "logs");
                DatabaseManagement.InitDatabase(connectionString, "quartz");

                DbMigrationsRunner.MigrateToLatest(connectionString, "events",
                    new DbUpgradeSettings(typeof(M201812181520_AddedGlobalSequenceSequence).Assembly,
                        typeof(M201812181520_AddedGlobalSequenceSequence).Namespace));
                DbMigrationsRunner.MigrateToLatest(connectionString, "plainstore",
                    new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace));
                DbMigrationsRunner.MigrateToLatest(connectionString, "readside",
                    new DbUpgradeSettings(typeof(M001_InitDb).Assembly, typeof(M001_InitDb).Namespace));
                DbMigrationsRunner.MigrateToLatest(connectionString, "users",
                    new DbUpgradeSettings(typeof(M001_AddUsersHqIdentityModel).Assembly,
                        typeof(M001_AddUsersHqIdentityModel).Namespace));
                DbMigrationsRunner.MigrateToLatest(connectionString, "logs",
                    new DbUpgradeSettings(typeof(M201905171139_AddErrorsTable).Assembly,
                        typeof(M201905171139_AddErrorsTable).Namespace));
                DbMigrationsRunner.MigrateToLatest(connectionString, "quartz",
                    new DbUpgradeSettings(typeof(M201905151013_AddQuartzTables).Assembly,
                        typeof(M201905151013_AddQuartzTables).Namespace));
            };
            
            Assert.DoesNotThrow(runner.Invoke);
        }

        [OneTimeTearDown]
        public void drop_db_on_finish_test()
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
            var dbToDelete = builder.Database;
            builder.Database = "postgres";
            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = string.Format(@"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};", dbToDelete); ;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
