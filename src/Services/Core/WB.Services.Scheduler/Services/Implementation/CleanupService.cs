using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Scheduler.Jobs;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class CleanupService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<JobSettings> options;
        private readonly ILogger<CleanupService> logger;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Task task;

        public CleanupService(IServiceProvider serviceProvider, IOptions<JobSettings> options, ILogger<CleanupService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.task = Task.Run(async () =>
            {
                logger.LogInformation("Cleanup service started");
                while (true)
                { 
                    if (cancellationToken.IsCancellationRequested) break;

                    using (var scope = this.serviceProvider.CreateScope())
                    {
                        var cleanup = scope.ServiceProvider.GetService<StaleJobCleanupService>();
                        await cleanup.ExecuteAsync(cancellationToken);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(options.Value.ClearStaleJobsInSeconds), cancellationToken);
                }
            }, cts.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Cleanup service stopping");
            this.cts.Cancel();
            try { await task;} catch { /* om om om: we just need to wait for task to complete */}
            logger.LogInformation("Cleanup service stopped");
        }
    }
}
