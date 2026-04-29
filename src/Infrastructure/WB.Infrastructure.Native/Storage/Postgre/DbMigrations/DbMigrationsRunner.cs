using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using WB.Core.Infrastructure.Exceptions;
using WB.Infrastructure.Native.Utils;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public static class DbMigrationsRunner
    {
        public static void MigrateToLatest(string connectionString, string schemaName,
            DbUpgradeSettings dbUpgradeSettings, ILoggerProvider loggerProvider = null, 
            IConfiguration configuration = null)
        {
            var npgConnBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            npgConnBuilder.CommandTimeout = 0;
            npgConnBuilder.SearchPath = schemaName;
            npgConnBuilder.SetApplicationPostfix("migrations");
            npgConnBuilder.Pooling = false;

            var serviceCollection = new ServiceCollection();
            if (loggerProvider != null)
            {
                serviceCollection.AddSingleton<ILoggerProvider>(loggerProvider);
            }
            else
            {
                serviceCollection.AddLogging(l => { l.AddFluentMigratorConsole(); });
            }

            var services = serviceCollection
                // Logging is the replacement for the old IAnnouncer
                .AddSingleton(new DefaultConventionSet(defaultSchemaName: null, workingDirectory: null))
                .Configure<ProcessorOptions>(opt => { opt.PreviewOnly = false; })
                .Configure<TypeFilterOptions>(opt => { opt.Namespace = dbUpgradeSettings.MigrationsNamespace; })
                // Registration of all FluentMigrator-specific services
                .AddFluentMigratorCore()
                // Configure the runner
                .ConfigureRunner(
                    builder => builder
                        // Add Postgres
                        .AddPostgres(schemaName)
                        // The Postgres connection string
                        .WithGlobalConnectionString(npgConnBuilder.ConnectionString)
                        // Specify the assembly with the migrations
                        .ScanIn(dbUpgradeSettings.MigrationsAssembly)
                        .For.Migrations()
                        .For.EmbeddedResources());

            if (configuration != null)
            {
                services.AddSingleton<IConfiguration>(configuration);
            }

            using var serviceProvider = services.BuildServiceProvider();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using var scope = serviceProvider.CreateScope();
            // Instantiate the runner
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            using var migrationLock = new MigrationLock(npgConnBuilder, false);

            // Check after acquiring the lock to avoid a race where another instance
            // migrates to a newer version between the check and MigrateUp().
            CheckDatabaseNotNewerThanApp(npgConnBuilder, schemaName, dbUpgradeSettings);

            // Execute the migrations
            runner.MigrateUp();
        }

        private static void CheckDatabaseNotNewerThanApp(NpgsqlConnectionStringBuilder connectionStringBuilder,
            string schemaName, DbUpgradeSettings dbUpgradeSettings)
        {
            long maxAppVersion = GetMaxMigrationVersion(dbUpgradeSettings);
            if (maxAppVersion == 0)
            {
                throw new InitializationException(Subsystem.Database,
                    $"Unable to determine the maximum supported database migration version for schema '{schemaName}'. " +
                    $"No migrations were discovered in assembly '{dbUpgradeSettings.MigrationsAssembly.FullName}' " +
                    $"under namespace '{dbUpgradeSettings.MigrationsNamespace}'. Refusing to start because the database may have been migrated by a newer version of Survey Solutions.");
            }

            using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            bool tableExists = connection.ExecuteScalar<bool>(
                "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = @schema AND table_name = 'VersionInfo')",
                new { schema = schemaName });

            if (!tableExists) return;

            var quotedSchemaName = new NpgsqlCommandBuilder().QuoteIdentifier(schemaName);
            long? maxDbVersion = connection.ExecuteScalar<long?>(
                $"SELECT MAX(\"Version\") FROM {quotedSchemaName}.\"VersionInfo\"");

            if (maxDbVersion.HasValue && maxDbVersion.Value > maxAppVersion)
            {
                throw new InitializationException(Subsystem.Database,
                    $"Database schema '{schemaName}' has been created or updated by a newer version of Survey Solutions. " +
                    $"The database contains migration version {maxDbVersion.Value}, but this application only supports up to version {maxAppVersion}. " +
                    "Please upgrade Survey Solutions to a newer version to use this database.");
            }
        }

        private static long GetMaxMigrationVersion(DbUpgradeSettings dbUpgradeSettings)
        {
            string ns = dbUpgradeSettings.MigrationsNamespace;
            IEnumerable<Type> types;
            try
            {
                types = dbUpgradeSettings.MigrationsAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.OfType<Type>();
            }

            return types
                .Where(t => t.Namespace != null
                            && (t.Namespace == ns || (ns != null && t.Namespace.StartsWith(ns + "."))))
                .Select(t => t.GetCustomAttribute<MigrationAttribute>())
                .Where(a => a != null)
                .Select(a => a!.Version)
                .DefaultIfEmpty(0)
                .Max();
        }
    }

}
