using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Services.Quartz
{
    internal class HqQuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly UnderConstructionInfo underConstructionInfo;
        private readonly IOptions<QuartzHostedServiceOptions> options;
        private IScheduler scheduler = null!;
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly IOptions<QuartzIntegrationExtensions.QuartzMigratorConfig> schedulerConfig;

        public HqQuartzHostedService(UnderConstructionInfo underConstructionInfo,
            ISchedulerFactory schedulerFactory,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IOptions<QuartzHostedServiceOptions> options,
            IOptions<QuartzIntegrationExtensions.QuartzMigratorConfig> schedulerConfig)
        {
            this.schedulerFactory = schedulerFactory;
            this.configuration = configuration;
            this.underConstructionInfo = underConstructionInfo;
            this.options = options;
            this.serviceProvider = serviceProvider;
            this.schedulerConfig = schedulerConfig;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // postponing quartz jobs from running until database is migrated
            Task.Factory.StartNew(async () =>
            {
                await underConstructionInfo.WaitForFinish;
                await using var migrationLock = new MigrationLock(configuration.GetConnectionString("DefaultConnection"));
                this.serviceProvider.RunQuartzMigrations(schedulerConfig.Value.DbUpgradeSetting);
                await this.serviceProvider.InitQuartzJobs();

                scheduler = await schedulerFactory.GetScheduler(cancellationToken);
                await scheduler.Start(cancellationToken);
            }, cancellationToken);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return scheduler.Shutdown(options.Value.WaitForJobsToComplete, cancellationToken);
        }
    }
}
