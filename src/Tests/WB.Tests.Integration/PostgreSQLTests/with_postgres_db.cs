using System;
using System.Configuration;
using Machine.Specifications;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class with_postgres_db
    {
        protected static void Context()
        {
            TestConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = string.Format(@"CREATE DATABASE {0} ENCODING = 'UTF8'", databaseName);
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        Establish context = () => Context();

        Cleanup things = () => Cleanup();

        protected static void Cleanup()
        {
            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = string.Format(
                    @"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};",
                    databaseName);
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected static NpgsqlConnectionStringBuilder connectionStringBuilder;
        protected static string TestConnectionString;
        private static string databaseName;
    }
}