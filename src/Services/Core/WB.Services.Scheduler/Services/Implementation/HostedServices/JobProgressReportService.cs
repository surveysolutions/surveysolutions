using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WB.Services.Scheduler.Services.Implementation.HostedServices
{
    internal class JobProgressReportService : IHostedSchedulerService
    {
        private readonly IServiceProvider serviceProvider;

        private IJobProgressReporter jobProgressReporter;
        private IServiceScope scope;

        public JobProgressReportService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.scope = serviceProvider.CreateScope();
            this.jobProgressReporter = scope.ServiceProvider.GetService<IJobProgressReporter>();
            this.jobProgressReporter.Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.jobProgressReporter.AbortAsync(cancellationToken);
            scope.Dispose();
        }
    }
}
