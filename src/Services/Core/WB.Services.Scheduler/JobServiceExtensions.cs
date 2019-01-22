﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Prometheus.Advanced;
using WB.Services.Infrastructure.Health;
using WB.Services.Scheduler.Services;
using WB.Services.Scheduler.Services.Implementation;
using WB.Services.Scheduler.Services.Implementation.HostedServices;
using WB.Services.Scheduler.Stats;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler
{
    public static class SchedulerGlobalConfiguration
    {
        public static readonly Dictionary<string, TypeInfo> JubRunnerHandlers = new Dictionary<string, TypeInfo>();

        public static void RegisterJob(TypeInfo type, string jobType)
        {
            JubRunnerHandlers.Add(jobType, type);
        }
    }

    public static class JobServiceExtensions
    {
        public static void UseJobService(this IServiceCollection services, IConfiguration configuration, string section = "job")
        {
            var jobSettingsSection = configuration?.GetSection(section);
            var connectionName = jobSettingsSection?[nameof(JobSettings.ConnectionName)];

            if (string.IsNullOrWhiteSpace(connectionName))
                connectionName = new JobSettings().ConnectionName;

            services.AddSingleton<IHostedService, BackgroundExportService>();

            services.AddTransient<IHostedSchedulerService, CleanupService>();
            services.AddTransient<IHostedSchedulerService, WorkCancellationTrackService>();
            services.AddTransient<IHostedSchedulerService, JobProgressReportService>();
            services.AddTransient<IHostedSchedulerService, JobWorkersManageService>();

            services.AddSingleton<IJobCancellationNotifier, JobCancellationNotifier>();

            services.ConfigureHealthCheck<DbHealthCheck>();
            services.ConfigureHealthCheck<EfCoreHealthCheck>();
            services.AddTransient<IJobService, JobService>();
            services.AddSingleton<IJobProgressReporter, JobProgressReporter>();
            services.AddTransient<IJobWorker, JobWorker>();
            services.AddTransient<IJobExecutor, JobExecutor>();

            services.AddDbContext<JobContext>(ops =>
                ops
                    .UseLoggerFactory(MyLoggerFactory)
                    .UseNpgsql(configuration.GetConnectionString(connectionName)));

            services.Configure<JobSettings>(jobSettingsSection);

            services.AddTransient<IOnDemandCollector, SchedulerStatsCollector>();

            services.RegisterJobHandler<StaleJobCleanupService>(StaleJobCleanupService.Name);
        }

        public static void RegisterJobHandler<THandler>(this IServiceCollection services, string name) where THandler : class
        {
            services.AddTransient<THandler>();
            SchedulerGlobalConfiguration.RegisterJob(typeof(THandler).GetTypeInfo(), name);
        }

        public static async Task RunJobServiceMigrations(this IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();
                await EnsurePublicSchemaExists(db.Database);
                await db.Database.MigrateAsync(cancellationToken);
            }
        }
        
        private static async Task EnsurePublicSchemaExists(DatabaseFacade db)
        {
            try
            {
                await db.GetDbConnection().ExecuteAsync("create schema if not exists public");
            }
            catch { /* 
                    If DB is not created, then db.Database.MigrateAsync will create it with public schema
                    but if there is already created DB without public schema, them MigrateAsync will fail.
                    So it's OK to fail here and om om om exception and fail later on Migrate if there is a 
                    problem with migrations or DB access
                 */ }
        }

        /// <summary>
        /// True random lock value for lock to prevent run of several migrations at once
        /// </summary>
        private const long LockValueForMigration = -889238397;

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[] { new ConsoleLoggerProvider((s, level) => (int)level >= 4, true) });
    }
}
