using System;
using System.Configuration;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.Events;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    public class with_postgres_db
    {
        protected class AccountRegistered : IEvent
        {
            public string ApplicationName { get; set; }
            public string ConfirmationToken { get; set; }
            public string Email { get; set; }
        }

        protected class AccountConfirmed : IEvent { }

        protected class AccountLocked : IEvent { }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            TestConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
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

            DatabaseManagement.InitDatabase(connectionStringBuilder.ConnectionString, schemaName);
            DbMigrationsRunner.MigrateToLatest(connectionStringBuilder.ConnectionString, schemaName,
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));
        }

        [OneTimeTearDown]
        public void TearDown()
        {

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = string.Format(@"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};", databaseName);
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected static NpgsqlConnectionStringBuilder connectionStringBuilder;
        private static string TestConnectionString;
        private static string databaseName;
        protected static string schemaName = "events";
    }
}
