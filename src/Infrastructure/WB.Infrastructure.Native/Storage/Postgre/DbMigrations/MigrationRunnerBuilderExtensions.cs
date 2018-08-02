using System;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public static class MigrationRunnerBuilderExtensions
    {

        public static IMigrationRunnerBuilder AddPostgres(this IMigrationRunnerBuilder migrationRunnerBuilder, string schemaName)
        {
            if (migrationRunnerBuilder == null) throw new ArgumentNullException(nameof(migrationRunnerBuilder));

            migrationRunnerBuilder.Services.AddScoped<PostgresDbFactory>()
                .AddScoped<PostgresProcessor>(sp => new InSchemaPostgresProcessor(schemaName,
                    sp.GetRequiredService<PostgresDbFactory>(),
                    sp.GetRequiredService<PostgresGenerator>(),
                    sp.GetRequiredService<ILogger<PostgresProcessor>>(),
                    sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>(),
                    sp.GetRequiredService<IConnectionStringAccessor>()))
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<PostgresProcessor>())
                .AddScoped<PostgresQuoter>()
                .AddScoped<PostgresGenerator>(provider => new InSchemaPostgresGenerator(schemaName))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<PostgresGenerator>());

            return migrationRunnerBuilder;
        }
    }
}
