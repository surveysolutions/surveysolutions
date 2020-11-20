using System;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.Events;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Persistence.Headquarters.Migrations.Users;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    internal class DatabaseTestInitializer
    {
        public static string CreateAndInitializeDb(params DbType[] dbType)
        {
            var TestConnectionString = TestsConfigurationManager.ConnectionString;
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

            InitializeDb(connectionStringBuilder.ConnectionString, dbType);

            return connectionStringBuilder.ConnectionString;
        }

        public static void InitializeDb(string connectionString, params DbType[] dbType)
        {
            foreach (var db in dbType)
            {
                string schemaName = db switch
                {
                    DbType.PlainStore => "plainstore",
                    DbType.ReadSide => "readside",
                    DbType.Users => "users",
                    _ => null
                };

                DatabaseManagement.InitDatabase(connectionString, schemaName);
                DatabaseManagement.InitDatabase(connectionString, "users");
                DatabaseManagement.InitDatabase(connectionString, "events");
                DbMigrationsRunner.MigrateToLatest(connectionString, "events",
                    new DbUpgradeSettings(typeof(M201812181520_AddedGlobalSequenceSequence).Assembly, typeof(M201812181520_AddedGlobalSequenceSequence).Namespace));

                switch (db)
                {
                    case DbType.PlainStore:
                        DbMigrationsRunner.MigrateToLatest(connectionString, schemaName,
                            new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace));
                        break;
                    case DbType.ReadSide:
                        DbMigrationsRunner.MigrateToLatest(connectionString, schemaName,
                            new DbUpgradeSettings(typeof(M001_InitDb).Assembly, typeof(M001_InitDb).Namespace));
                        break;
                    case DbType.Users:
                        DbMigrationsRunner.MigrateToLatest(connectionString, schemaName,
                            new DbUpgradeSettings(typeof(M001_AddUsersHqIdentityModel).Assembly, typeof(M001_AddUsersHqIdentityModel).Namespace));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
