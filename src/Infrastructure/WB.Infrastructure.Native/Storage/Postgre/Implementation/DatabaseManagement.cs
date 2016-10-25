using Npgsql;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal static class DatabaseManagement
    {
        public static void InitDatabase(string connectionString, string schemaName)
        {
            CreateDatabase(connectionString);
            CreateSchema(connectionString, schemaName);
        }

        private static void CreateDatabase(string connectionString)
        {
            var masterDbConnectionString = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = masterDbConnectionString.Database;
            masterDbConnectionString.Database = "postgres"; // System DB name.

            using (var connection = new NpgsqlConnection(masterDbConnectionString.ConnectionString))
            {
                connection.Open();
                var checkDbExistsCommand = connection.CreateCommand();
                checkDbExistsCommand.CommandText = "SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower(:dbName);";
                checkDbExistsCommand.Parameters.AddWithValue("dbName", databaseName);
                var dbExists = checkDbExistsCommand.ExecuteScalar();

                if (dbExists == null)
                {
                    var createCommand = connection.CreateCommand();
                    createCommand.CommandText = $@"CREATE DATABASE ""{databaseName}"" ENCODING = 'UTF8'";
                        // unfortunately there is no way to use parameters based syntax here 
                    createCommand.ExecuteNonQuery();
                }
            }
        }

        private static void CreateSchema(string connectionString, string schemaName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var checkSchemaExistsCommand = connection.CreateCommand();
                checkSchemaExistsCommand.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
                checkSchemaExistsCommand.ExecuteNonQuery();
            }
        }
    }
}