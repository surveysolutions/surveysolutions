using System;
using Dapper;
using FluentMigrator;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;
using Npgsql;
using WB.Infrastructure.Native.Utils;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public static class DatabaseManagement
    {
        public static void InitDatabase(string connectionString, string schemaName)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            connectionStringBuilder.SetApplicationPostfix("init");
            connectionStringBuilder.Pooling = false;
            CreateDatabase(connectionStringBuilder.ConnectionString);
            CreateSchema(connectionStringBuilder.ConnectionString, schemaName);
        }

        public static void CreateDatabase(string connectionString)
        {
            var masterDbConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = masterDbConnectionStringBuilder.Database;
            masterDbConnectionStringBuilder.Database = "postgres"; // System DB name.

            using var @lock = new MigrationLock(masterDbConnectionStringBuilder);
            using var connection = new NpgsqlConnection(masterDbConnectionStringBuilder.ConnectionString);
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

        private static void CreateSchema(string connectionString, string schemaName)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            var checkSchemaExistsCommand = connection.CreateCommand();
            checkSchemaExistsCommand.CommandText = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"";
            checkSchemaExistsCommand.ExecuteNonQuery();
        }

        public static IFluentSyntax CreateTableIfNotExists(this MigrationBase self, string tableName,
            Func<ICreateTableWithColumnOrSchemaOrDescriptionSyntax, IFluentSyntax> constructTableFunction)
        {
            return !self.Schema.Table(tableName).Exists() ? constructTableFunction(self.Create.Table(tableName)) : null;
        }

        public static IFluentSyntax CreateForeignKeyFromTableIfNotExists(this MigrationBase self, string constraintName,
            string tableName,
            Func<ICreateForeignKeyForeignColumnOrInSchemaSyntax, IFluentSyntax> constructTableFunction)
        {
            return !self.Schema.Table(tableName).Constraint(constraintName).Exists()
                ? constructTableFunction(self.Create.ForeignKey(constraintName).FromTable(tableName))
                : null;
        }

        public static bool MigratedToWorkspaces(string workspaceSchemaName, string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            var versionInfoExists = connection.ExecuteScalar<bool>(
                "select exists (select 1 FROM information_schema.tables WHERE table_schema = :schemaName AND table_name = 'VersionInfo')",
                new {schemaName = workspaceSchemaName});

            if (versionInfoExists)
            {
                int migrationsCount = connection.ExecuteScalar<int>($"SELECT COUNT(\"Version\") FROM {workspaceSchemaName}.\"VersionInfo\"");
                return migrationsCount >= 2;
            }
            
            return false;
        }
    }
}
