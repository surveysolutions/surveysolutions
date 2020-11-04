using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class BackgroundExportService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private IEnumerable<IHostedSchedulerService>? backgroundServices;
        private readonly ILogger<BackgroundExportService> logger;
        private IServiceScope? scope;
        private CancellationTokenSource? cts;

        public BackgroundExportService(IServiceProvider serviceProvider,
            ILogger<BackgroundExportService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.scope = this.serviceProvider.CreateScope();

            cts = new CancellationTokenSource();
            await Policy.Handle<NpgsqlException>(c => c.IsTransient)
                .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(Math.Max(i, 5)))
                .ExecuteAsync(async ct =>
                {
                    if (ct.IsCancellationRequested) return;

                    using var migrationScope = this.serviceProvider.CreateScope();
                    await migrationScope.ServiceProvider.GetRequiredService<IJobContextMigrator>()
                        .MigrateAsync(ct);
                }, cancellationToken);

            this.backgroundServices = scope.ServiceProvider.GetServices<IHostedSchedulerService>();

            foreach (var backgroundService in backgroundServices)
            {
                if(cancellationToken.IsCancellationRequested) return;
                await backgroundService.StartAsync(cts.Token);
                logger.LogInformation("Started background service: {name}", backgroundService.GetType().Name);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cts?.Cancel();
            if (backgroundServices != null)
            {
                foreach (var backgroundService in backgroundServices)
                {
                    if (backgroundService == null) continue;

                    await backgroundService.StopAsync(cancellationToken);
                    logger.LogInformation("Stopped background service: {name}", backgroundService.GetType().Name);
                }
            }

            cts?.Dispose();
            this.scope?.Dispose();
        }
    }
}
