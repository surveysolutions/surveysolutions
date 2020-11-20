using System;
using System.Configuration;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class with_postgres_db
    {
        [OneTimeSetUp]
        protected void Context()
        {
            TestConnectionString = TestsConfigurationManager.ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            ConnectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = $"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
            using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = command;
            sqlCommand.ExecuteNonQuery();
        }

        [OneTimeTearDown]
        protected void Cleanup()
        {
            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = string.Format(
                @"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};",
                databaseName);
            using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = command;
            sqlCommand.ExecuteNonQuery();
        }

        public void InitializeDb( params DbType[] dbType)
        {
            DatabaseTestInitializer.InitializeDb(ConnectionStringBuilder.ConnectionString, dbType);
        }

        protected static NpgsqlConnectionStringBuilder ConnectionStringBuilder;
        static string TestConnectionString;
        private static string databaseName;
    }
}
