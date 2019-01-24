using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace WB.Services.Scheduler.Storage
{
    public class EfCoreHealthCheck : IHealthCheck
    {
        private readonly IOptions<JobSettings> jobSettings;
        private readonly DbContextOptions<JobContext> contextOptions;

        public EfCoreHealthCheck(IOptions<JobSettings> jobSettings, DbContextOptions<JobContext> contextOptions)
        {
            this.jobSettings = jobSettings;
            this.contextOptions = contextOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var db = new JobContext(contextOptions, jobSettings))
            {
                await db.Jobs.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
        }
    }
}
