using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Services.Scheduler.Services.Implementation.HostedServices
{
    internal class CleanupService : IHostedSchedulerService
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
                while (true)
                { 
                    logger.LogTrace("Start cleaning of stale running jobs");
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
            this.cts.Cancel();
            try { await task;} catch { /* om om om: we just need to wait for task to complete */}
        }
    }
}
