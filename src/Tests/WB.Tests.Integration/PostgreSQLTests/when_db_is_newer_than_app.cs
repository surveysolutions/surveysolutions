using System;
using Dapper;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Exceptions;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.PlainStore;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [TestFixture]
    public class when_db_is_newer_than_app
    {
        private string connectionString;
        private const string SchemaName = "plainstore";

        [OneTimeSetUp]
        public void create_empty_database()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var testConnectionString = TestsConfigurationManager.ConnectionString;
            var databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(testConnectionString)
            {
                Database = databaseName
            };

            using (var connection = new NpgsqlConnection(testConnectionString))
            {
                connection.Open();
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = $"CREATE DATABASE \"{databaseName}\" ENCODING = 'UTF8'";
                    sqlCommand.ExecuteNonQuery();
                }
            }

            connectionString = connectionStringBuilder.ConnectionString;

            // Create schema and VersionInfo table with a version higher than anything the app knows
            DatabaseManagement.InitDatabase(connectionString, SchemaName);

            using var conn = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString)
            {
                SearchPath = SchemaName,
                Pooling = false
            }.ConnectionString);
            conn.Open();

            conn.Execute($@"CREATE TABLE IF NOT EXISTS ""{SchemaName}"".""VersionInfo"" (
                ""Version"" bigint NOT NULL,
                ""AppliedOn"" timestamp,
                ""Description"" varchar(1024)
            )");

            // Insert a version that is far in the future — higher than any real migration
            conn.Execute($@"INSERT INTO ""{SchemaName}"".""VersionInfo"" (""Version"", ""AppliedOn"", ""Description"")
                VALUES (99999999999999, NOW(), 'Future migration from a newer app version')");
        }

        [Test]
        public void then_MigrateToLatest_should_throw_InitializationException_for_database_subsystem()
        {
            var settings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace);

            Action act = () => DbMigrationsRunner.MigrateToLatest(connectionString, SchemaName, settings);

            act.Should().Throw<InitializationException>()
                .Which.Subsystem.Should().Be(Subsystem.Database);
        }

        [Test]
        public void then_exception_message_should_contain_both_versions()
        {
            var settings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace);

            Action act = () => DbMigrationsRunner.MigrateToLatest(connectionString, SchemaName, settings);

            act.Should().Throw<InitializationException>()
                .WithMessage("*99999999999999*")
                .And.WithMessage("*plainstore*");
        }

        [OneTimeTearDown]
        public void drop_db_on_finish_test()
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var dbToDelete = builder.Database;
            builder.Database = "postgres";
            using var connection = new NpgsqlConnection(builder.ConnectionString);
            connection.Open();
            using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = $"DROP DATABASE \"{dbToDelete}\" WITH (FORCE);";
            sqlCommand.ExecuteNonQuery();
        }
    }
}
