using System;
using System.Configuration;
using Machine.Specifications;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class with_postgres_db
    {
        Establish context = () =>
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
        };

        Cleanup things = () =>
        {

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = @"DROP DATABASE " + databaseName;
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        };

        protected static NpgsqlConnectionStringBuilder connectionStringBuilder;
        private static string TestConnectionString;
        private static string databaseName;
    }
}