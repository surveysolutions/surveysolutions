using System;
using System.Configuration;
using Npgsql;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Tests.Integration
{
    internal abstract class NpgsqlTestContext
    {
        protected static NpgsqlConnectionStringBuilder connectionStringBuilder;
        protected static string TestConnectionString;
        private static string databaseName;

        protected IUnitOfWork UnitOfWork;

        [OneTimeSetUp]
        public void Context()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            TestConnectionString = TestsConfigurationManager.ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = $@"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
            using (var sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }
            connection.Close();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (UnitOfWork != null)
            {
                UnitOfWork.AcceptChanges();
                UnitOfWork.Dispose();
            }

            //pgSqlConnection.Close();

            using var connection = new NpgsqlConnection(TestConnectionString);
            connection.Open();
            var command = $@"DROP DATABASE {databaseName} WITH (FORCE);";
            using (var sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}
