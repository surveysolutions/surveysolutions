using System;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class with_postgres_db
    {
        [OneTimeSetUp]
        protected void Context()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            workspace = Create.Service.WorkspaceContextAccessor();
            TestConnectionString = TestsConfigurationManager.ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            ConnectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = $"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
            using (var sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }
        }

        protected IWorkspaceContextAccessor workspace;

        [OneTimeTearDown]
        protected void Cleanup()
        {
            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = $@"DROP DATABASE {databaseName} WITH (FORCE);";
            using (var sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public void InitializeDb( params DbType[] dbType)
        {
            DatabaseTestInitializer.InitializeDb(ConnectionStringBuilder.ConnectionString, workspace, dbType);
        }

        protected static NpgsqlConnectionStringBuilder ConnectionStringBuilder;
        static string TestConnectionString;
        private static string databaseName;
    }
}
