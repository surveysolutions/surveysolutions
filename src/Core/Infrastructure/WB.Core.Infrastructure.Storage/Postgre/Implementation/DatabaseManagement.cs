using Npgsql;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal static class DatabaseManagement
    {
        public static void CreateDatabase(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database; 
            builder.Database = "postgres"; // System DB name.

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                var checkDbExistsCommand = connection.CreateCommand();
                checkDbExistsCommand.CommandText = "SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower(:dbName);";
                checkDbExistsCommand.Parameters.AddWithValue("dbName", databaseName);
                var dbExists = checkDbExistsCommand.ExecuteScalar();

                if (dbExists == null)
                {
                    var createCommand = connection.CreateCommand();
                    createCommand.CommandText = string.Format(@"CREATE DATABASE ""{0}"" ENCODING = 'UTF8'", databaseName); // unfortunately there is no way to use parameters based syntax here 
                    createCommand.ExecuteNonQuery();
                }
            }
        }
    }
}