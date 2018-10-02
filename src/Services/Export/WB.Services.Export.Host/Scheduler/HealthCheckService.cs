using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Checks;

namespace WB.Services.Export.Host.Scheduler
{
    public class HealthCheckService : IHostedService
    {
        private readonly ILogger<HealthCheckService> logger;
        private readonly IEnumerable<IHealthCheck> checks;

        public HealthCheckService(ILogger<HealthCheckService> logger, IEnumerable<IHealthCheck> checks)
        {
            this.logger = logger;
            this.checks = checks;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var check in checks)
            {
                try
                {
                    await check.CheckAsync();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Startup Health Check failed");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}