using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Utils;

namespace WB.UI.Headquarters.Services.Quartz
{
    public static class QuartzIntegrationExtensions
    {
        public static void AddQuartzIntegration(this IServiceCollection services, 
            IConfiguration configuration, 
            DbUpgradeSettings dbUpgradeSettings)
        {
            var schedulerSection = configuration.GetSection("Scheduler").Get<SchedulerConfig>();
            services.AddHostedService<QuartzMigrator>();

            services.Configure<QuartzMigratorConfig>(c =>
            {
                c.DbUpgradeSetting = dbUpgradeSettings;
            });

            services.AddQuartz(q =>
            {
                q.SchedulerId = "Headquarters";
                q.UseJobFactory<AsyncScopedJobFactory>();
                q.UseDefaultThreadPool();

                q.UsePersistentStore(c =>
                {
                    c.UsePostgres(a =>
                    {
                        var connection = configuration.GetConnectionString("DefaultConnection");
                        var connectionBuilder = new NpgsqlConnectionStringBuilder(connection);
                        connectionBuilder.SetApplicationPostfix("quartz");
                        
                        a.ConnectionString = connectionBuilder.ConnectionString;
                        a.TablePrefix = "quartz.";
                    });

                    c.UseProperties = true;

                    if (schedulerSection.IsClustered)
                    {
                        c.UseClustering();
                    }

                    c.UseJsonSerializer();
                });
            });

            global::Quartz.Logging.LogProvider.IsDisabled = true;//.SetCurrentLogProvider(loggerFactory);

            services.AddSingleton<IScheduler>(s => 
                s.GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult());

            if (configuration["no-quartz"].ToBool(false) == false)
            {
                services.AddQuartzHostedService(q => { q.WaitForJobsToComplete = false; });
            }
        }

        public static IServiceCollection AddQuartzHostedService(
            this IServiceCollection services,
            Action<QuartzHostedServiceOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            
            return services.AddSingleton<IHostedService, HqQuartzHostedService>();
        }

        public static void RunQuartzMigrations(this IServiceProvider services, DbUpgradeSettings dbUpgradeSettings)
        {
            var migrationSettings = services.GetRequiredService<UnitOfWorkConnectionSettings>();
            
            DatabaseManagement.InitDatabase(migrationSettings.ConnectionString, "quartz");
            DbMigrationsRunner.MigrateToLatest(migrationSettings.ConnectionString, "quartz", dbUpgradeSettings,
                services.GetRequiredService<ILoggerProvider>());
        }

        public static async Task InitQuartzJobs(this IServiceProvider services)
        {
            var scheduler = services.GetRequiredService<IScheduler>();

            var jobSetting = services.GetRequiredService<SyncPackagesProcessorBackgroundJobSetting>();
            var importSettings = services.GetRequiredService<AssignmentImportOptions>();

            await services.GetRequiredService<InterviewDetailsBackgroundSchedulerTask>()
                .Schedule(repeatIntervalInSeconds: jobSetting.SynchronizationInterval);
            await services.GetRequiredService<UsersImportTask>().ScheduleRunAsync();
            await services.GetRequiredService<AssignmentsImportTask>().Schedule(repeatIntervalInSeconds: 300);
            await services.GetRequiredService<AssignmentsVerificationTask>().Schedule(repeatIntervalInSeconds: 300);
            await services.GetRequiredService<DeleteQuestionnaireJobScheduler>()
                    .Schedule(repeatIntervalInSeconds: 250);
            await services.GetRequiredService<UpgradeAssignmentJobScheduler>()
                .Schedule(importSettings.BackgroundExportIntervalInSeconds);
            await services.GetRequiredService<SendInvitationsTask>().ScheduleRunAsync();
            await services.GetRequiredService<SendRemindersTask>().Schedule(repeatIntervalInSeconds: 60 * 60);
            await services.GetRequiredService<SendInterviewCompletedTask>().Schedule(repeatIntervalInSeconds: 60);

            
            await scheduler.AddJob(DeleteWorkspaceSchemaJob.JobDetail(), true);
        }

        private class QuartzMigratorConfig
        {
            public DbUpgradeSettings DbUpgradeSetting { get; set; }
        }

        private class QuartzMigrator : IHostedService
        {
            private readonly IServiceProvider serviceProvider;
            private readonly IOptions<QuartzMigratorConfig> schedulerConfig;

            public QuartzMigrator(IServiceProvider serviceProvider, IOptions<QuartzMigratorConfig> schedulerConfig)
            {
                this.serviceProvider = serviceProvider;
                this.schedulerConfig = schedulerConfig;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                this.serviceProvider.RunQuartzMigrations(schedulerConfig.Value.DbUpgradeSetting);
                await this.serviceProvider.InitQuartzJobs();
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
