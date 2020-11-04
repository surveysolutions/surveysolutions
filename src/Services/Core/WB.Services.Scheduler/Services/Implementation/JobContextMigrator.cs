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
                        jobContext.Database.GetDbConnection().Execute("create schema if not exists public");
                        logger.LogInformation("Created {schema} before migration", "public");
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
                    await jobContext.Database.MigrateAsync(token);
                    logger.LogInformation("Job scheduler schema migration completed");
                });
        }
    }
}
