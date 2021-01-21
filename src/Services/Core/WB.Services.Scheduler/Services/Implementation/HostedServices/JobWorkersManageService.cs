using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Services.Scheduler.Services.Implementation.HostedServices
{
    internal class JobWorkersManageService : IHostedSchedulerService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<JobSettings> options;
        private readonly ILogger<BackgroundExportService> logger;

        private IServiceScope? scope;
        //private IJobWorker[] workers;

        public JobWorkersManageService(
            IServiceProvider serviceProvider,
            IOptions<JobSettings> options,
            ILogger<BackgroundExportService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            scope = serviceProvider.CreateScope();

            var workersCount = this.options.Value.WorkerCount;

            if (workersCount == 0)
            {
                workersCount = 1;
            }

            logger.LogTrace("Starting " + workersCount + " workers");

            var workers = new IJobWorker[workersCount];

            for (int i = 0; i < workers.Length; i++)
            {
                var worker = scope.ServiceProvider.GetRequiredService<IJobWorker>();
                workers[i] = worker;
                worker.Task = worker.StartAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
            this.scope?.Dispose();
            return Task.CompletedTask;
        }
    }
}
