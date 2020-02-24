using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Headquarters.HealthChecks
{
    public class HeadquartersStartupCheck : IHealthCheck
    {
        private readonly UnderConstructionInfo underConstructionInfo;

        public HeadquartersStartupCheck(UnderConstructionInfo underConstructionInfo)
        {
            this.underConstructionInfo = underConstructionInfo;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (underConstructionInfo.Status == UnderConstructionStatus.Finished)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            if (underConstructionInfo.Status == UnderConstructionStatus.Error)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(underConstructionInfo.Message));
            }

            return Task.FromResult(HealthCheckResult.Degraded(underConstructionInfo.Message));
        }
    }
}
