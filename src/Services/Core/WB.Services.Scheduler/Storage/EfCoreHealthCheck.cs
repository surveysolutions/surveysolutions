using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WB.Services.Scheduler.Storage
{
    public class EfCoreHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider serviceProvider;

        public EfCoreHealthCheck(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            await using var db = serviceProvider.GetRequiredService<JobContext>();
            await db.Jobs.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
    }
}
