using System;
using System.Configuration;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.ReadSide;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    internal enum DbType
    {
        PlainStore,
        ReadSide
    }

    internal class DatabaseTestInitializer
    {
        public static string InitializeDb(params DbType[] dbType)
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

            foreach (var db in dbType)
            {
                string schemaName = null;
                switch (db)
                {
                    case DbType.PlainStore:
                        schemaName = "plainstore";
                        break;
                    case DbType.ReadSide:
                        schemaName = "readside";
                        break;
                }

                DatabaseManagement.InitDatabase(connectionStringBuilder.ConnectionString, schemaName);
                DatabaseManagement.InitDatabase(connectionStringBuilder.ConnectionString, "users");

                switch (db)
                {
                    case DbType.PlainStore:
                        DbMigrationsRunner.MigrateToLatest(connectionStringBuilder.ConnectionString, schemaName,
                            new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace));
                        break;
                    case DbType.ReadSide:
                        DbMigrationsRunner.MigrateToLatest(connectionStringBuilder.ConnectionString, schemaName,
                            new DbUpgradeSettings(typeof(M001_InitDb).Assembly, typeof(M001_InitDb).Namespace));
                        break;
                }
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
