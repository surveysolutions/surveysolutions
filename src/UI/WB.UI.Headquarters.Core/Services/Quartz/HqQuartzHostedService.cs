using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Headquarters.Services.Quartz
{
    internal class HqQuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly UnderConstructionInfo underConstructionInfo;
        private readonly IOptions<QuartzHostedServiceOptions> options;
        private IScheduler scheduler = null!;

        public HqQuartzHostedService(
            ISchedulerFactory schedulerFactory,
            UnderConstructionInfo underConstructionInfo,
            IOptions<QuartzHostedServiceOptions> options)
        {
            this.schedulerFactory = schedulerFactory;
            this.underConstructionInfo = underConstructionInfo;
            this.options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // postponing quartz jobs from running until database is migrated
            Task.Factory.StartNew(async () =>
            {
                await underConstructionInfo.WaitForFinish;
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