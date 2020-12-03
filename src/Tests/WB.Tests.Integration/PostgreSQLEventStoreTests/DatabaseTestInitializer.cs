﻿using System;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Workspaces;
using WB.Persistence.Headquarters.Migrations.Events;
using WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Tests.Abc;
using M202011201421_InitSingleWorkspace = WB.Persistence.Headquarters.Migrations.Workspace.M202011201421_InitSingleWorkspace;

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

            InitializeDb(connectionStringBuilder.ConnectionString, Create.Service.WorkspaceContextAccessor(), dbType);

            return connectionStringBuilder.ConnectionString;
        }

        public static void InitializeDb(string connectionString, IWorkspaceContextAccessor workspaceContextAccessor, params DbType[] dbType)
        {
            DatabaseManagement.InitDatabase(connectionString, "users");
            DatabaseManagement.InitDatabase(connectionString, "events");
            DbMigrationsRunner.MigrateToLatest(connectionString, "events",
                new DbUpgradeSettings(typeof(M201812181520_AddedGlobalSequenceSequence).Assembly, typeof(M201812181520_AddedGlobalSequenceSequence).Namespace));
           
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

                DatabaseManagement.InitDatabase(connectionString, schemaName);
              
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
                }
            }

            var schema = workspaceContextAccessor.CurrentWorkspace().SchemaName;
            DatabaseManagement.InitDatabase(connectionString, schema);
            DbMigrationsRunner.MigrateToLatest(connectionString, schema,
               DbUpgradeSettings.FromFirstMigration<M202011131055_MoveOldSchemasToWorkspace>());

            DbMigrationsRunner.MigrateToLatest(connectionString, schema,
                DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());

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
