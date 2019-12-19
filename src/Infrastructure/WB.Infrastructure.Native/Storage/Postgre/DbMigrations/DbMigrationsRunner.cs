﻿using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public static class DbMigrationsRunner
    {
        public static void MigrateToLatest(string connectionString, string schemaName,
            DbUpgradeSettings dbUpgradeSettings)
        {
            var npgConnBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            npgConnBuilder.CommandTimeout = 0;

            var serviceProvider = new ServiceCollection()
                // Logging is the replacement for the old IAnnouncer
                .AddSingleton<ILoggerProvider, NLogLoggerProvider>()
                .AddSingleton<IServiceLocator>((s) => ServiceLocator.Current.GetInstance<IServiceLocator>())
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
                        .For.EmbeddedResources())
                .BuildServiceProvider();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                
                // Instantiate the runner
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }
    }

}
