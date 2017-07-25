using System;
using System.Configuration;
using System.Data.Entity;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.Users;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    internal enum DbType
    {
        PlainStore
    }

    internal class DatabaseTestInitializer
    {
        public static string InitializeDb(DbType dbType)
        {
            var TestConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
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
            string schemaName = null;
            switch (dbType)
            {
                case DbType.PlainStore:
                    schemaName = "plainstore";
                    break;
            }

            DatabaseManagement.InitDatabase(connectionStringBuilder.ConnectionString, schemaName);
            DatabaseManagement.InitDatabase(connectionStringBuilder.ConnectionString, "users");

            switch (dbType)
            {
                case DbType.PlainStore:
                    DbMigrationsRunner.MigrateToLatest(connectionStringBuilder.ConnectionString, schemaName,
                        new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace));
                    break;
            }

            return connectionStringBuilder.ConnectionString;
        }

        public static void DropDb(string connectionString)
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