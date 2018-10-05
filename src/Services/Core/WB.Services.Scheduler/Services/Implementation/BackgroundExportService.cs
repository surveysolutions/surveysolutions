using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class BackgroundExportService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private IJobProgressReporter jobProgressReporter;
        private IServiceScope scope;
        private readonly IOptions<JobSettings> options;
        private readonly ILogger<BackgroundExportService> logger;
        private IJobWorker[] workers;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public BackgroundExportService(IServiceProvider serviceProvider,
            IOptions<JobSettings> options,
            ILogger<BackgroundExportService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await serviceProvider.RunJobServiceMigrations();

            logger.LogInformation("Background export service workers start");

            scope = serviceProvider.CreateScope();

            this.jobProgressReporter = scope.ServiceProvider.GetService<IJobProgressReporter>();
            this.jobProgressReporter.Start();

            var workersCount = this.options.Value.WorkerCount;

            if (workersCount == 0)
            {
                workersCount = Environment.ProcessorCount;
            }

            workers = new IJobWorker[workersCount];

            for (int i = 0; i < workers.Length; i++)
            {
                var worker = serviceProvider.GetService<IJobWorker>();
                workers[i] = worker;
                worker.Task = worker.StartAsync(cts.Token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            scope.Dispose();
            logger.LogInformation("Background export service workers stopped");
            cts.Cancel();
            return Task.WhenAll(workers.Select(w => w.Task));
        }
    }
}
