using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class BackgroundExportService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private IEnumerable<IHostedSchedulerService> backgroundServices;
        private readonly ILogger<BackgroundExportService> logger;
        private IServiceScope scope;

        public BackgroundExportService(IServiceProvider serviceProvider,
            ILogger<BackgroundExportService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await serviceProvider.RunJobServiceMigrations();

            this.scope = this.serviceProvider.CreateScope();
            this.backgroundServices = scope.ServiceProvider.GetServices<IHostedSchedulerService>();

            foreach (var backgroundService in backgroundServices)
            {
                await backgroundService.StartAsync(cancellationToken);
                logger.LogInformation("Started background service: " + backgroundService.GetType().Name);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var backgroundService in backgroundServices)
            {
                await backgroundService.StopAsync(cancellationToken);
                logger.LogInformation("Stopped background service: " + backgroundService.GetType().Name);
            }

            this.scope.Dispose();
        }
    }
}
