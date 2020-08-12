using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class EmbeddedExportServiceHealthCheck : IHealthCheck
    {
        private volatile bool _startupTaskCompleted = false;
        public bool StartupTaskCompleted
        {
            get => _startupTaskCompleted;
            set => _startupTaskCompleted = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (StartupTaskCompleted)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy(Diagnostics.embedded_export_service_check_Healty));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy(Diagnostics.embedded_export_service_check_Degraded));
        }
    }
}
