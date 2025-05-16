using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobContextMigrator : IJobContextMigrator
    {
        private readonly JobContext jobContext;
        private readonly ILogger<JobContextMigrator> logger;

        public JobContextMigrator(JobContext jobContext, ILogger<JobContextMigrator> logger)
        {
            this.jobContext = jobContext;
            this.logger = logger;
        }

        Random rnd = new Random();

        public async Task MigrateAsync(CancellationToken token)
        {
            if (!await jobContext.Database.CanConnectAsync(token))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(rnd.NextDouble() * 2000), token);
            }

            await Polly.Policy.Handle<Exception>(e =>
                {
                    if (e is PostgresException pe && pe.SqlState == "3F000") // 3F000: no schema has been selected to create in
                    {
                        // if Database is already created, but do not contains public schema - then EF Migration will fail
                        jobContext.Database.GetDbConnection().Execute($"create schema if not exists {JobSettings.MigrationsSchemaName}");
                        logger.LogInformation("Created {schema} before migration", JobSettings.MigrationsSchemaName);
                    }
                    else
                    {
                        logger.LogWarning("Were not able to run migration: {message}, {exception}", e.Message, e);
                    }

                    return true;
                })
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(rnd.NextDouble() * i + i))
                .ExecuteAsync(async () =>
                {
                    await MoveMigrationTableAsync(token);
                    await jobContext.Database.MigrateAsync(token);
                    logger.LogInformation("Job scheduler schema migration completed");
                });
        }
        
        public async Task MoveMigrationTableAsync(CancellationToken cancellationToken)
        {
            var script = @"
DO $$ 
DECLARE
    current_schema_name text;
BEGIN
    SELECT current_schema() INTO current_schema_name;

    IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'es_migrations') THEN
        EXECUTE 'CREATE SCHEMA es_migrations';
    END IF;

    IF EXISTS (SELECT 1 
               FROM information_schema.tables 
               WHERE table_schema = current_schema_name 
               AND table_name = '__EFMigrationsHistory') THEN
       
        IF NOT EXISTS (SELECT 1 
                       FROM information_schema.tables 
                       WHERE table_schema = 'es_migrations' 
                       AND table_name = '__EFMigrationsHistory') THEN

            EXECUTE format('ALTER TABLE %I.""__EFMigrationsHistory"" SET SCHEMA es_migrations', current_schema_name);
        END IF;
    END IF;
END $$;
";

            try
            {
                await jobContext.Database.ExecuteSqlRawAsync(script, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is PostgresException pe && pe.SqlState == "3D000")
                    return;
                
                logger.LogError(ex, $"An error occurred while moving the table: {ex.Message}");
                throw;
            }
        }
    }
    
}
