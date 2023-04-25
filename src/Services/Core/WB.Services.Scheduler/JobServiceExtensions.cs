#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Scheduler.Services;
using WB.Services.Scheduler.Services.Implementation;
using WB.Services.Scheduler.Services.Implementation.HostedServices;

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
            var jobSettingsSection = configuration.GetSection(section);
            var connectionName = jobSettingsSection[nameof(JobSettings.ConnectionName)];
            if (string.IsNullOrWhiteSpace(connectionName))
                connectionName = new JobSettings().ConnectionName;
            
            var connectionString = configuration.GetConnectionString(connectionName);
            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Connection string was not found");

            services.AddHostedService<BackgroundExportService>();

            services.AddTransient<IHostedSchedulerService, CleanupService>();
            services.AddTransient<IHostedSchedulerService, WorkCancellationTrackService>();
          
            services.AddTransient<IHostedSchedulerService, JobWorkersManageService>();

            services.AddSingleton<IJobCancellationNotifier, JobCancellationNotifier>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IJobWorker, JobWorker>();
            services.AddTransient<IJobExecutor, JobExecutor>();
            services.AddProgressReporter();
            services.AddTransient<IJobContextMigrator, JobContextMigrator>();
            services.AddDbContext<JobContext>(ops =>
                ops.UseNpgsql(connectionString));

            services.Configure<JobSettings>(jobSettingsSection);

            services.RegisterJobHandler<StaleJobCleanupService>(StaleJobCleanupService.Name);

            services.AddTransient<IJobsArchiver, JobsArchiver>();
        }

        public static void AddProgressReporter(this IServiceCollection services)
        {
            services.AddSingleton<Services.IJobProgressReporter, JobProgressReporterBackgroundService>();
            services.AddHostedService(sl=> sl.GetService<Services.IJobProgressReporter>() as JobProgressReporterBackgroundService 
                                           ?? throw new InvalidOperationException($"Invalid service for {nameof(JobProgressReporterBackgroundService)}"));

            services.AddTransient<IJobProgressReportWriter, JobProgressReportWriter>();
        }

        public static void RegisterJobHandler<THandler>(this IServiceCollection services, string name) where THandler : class
        {
            services.AddTransient<THandler>();
            SchedulerGlobalConfiguration.RegisterJob(typeof(THandler).GetTypeInfo(), name);
        }
    }
}
