using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation;

namespace WB.Services.Export.Host.Scheduler
{
    class BackgroundExportService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IStaleJobCleanup jobCleanup;
        private IJobProgressReporter jobProgressReporter;
        private IServiceScope scope;
        private readonly IOptions<JobSettings> options;
        private readonly ILogger<BackgroundExportService> logger;
        private IJobWorker[] workers;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public BackgroundExportService(IServiceProvider serviceProvider,
            IStaleJobCleanup jobCleanup,
            IOptions<JobSettings> options,
            ILogger<BackgroundExportService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.jobCleanup = jobCleanup;
            this.options = options;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background export service workers start");

            scope = serviceProvider.CreateScope();

            await jobCleanup.ScheduleIfNeeded();
                
            this.jobProgressReporter = scope.ServiceProvider.GetService<IJobProgressReporter>();
            this.jobProgressReporter.Start();

            workers = new IJobWorker[this.options.Value.WorkerCount];

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
