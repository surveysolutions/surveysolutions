using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class ExportServiceConnectivityCheck : IHealthCheck
    {
        private readonly IExportServiceApi exportServiceApi;
        private readonly IOptions<DataExportOptions> exportOptions;

        public ExportServiceConnectivityCheck(IExportServiceApi exportServiceApi, IOptions<DataExportOptions> exportOptions)
        {
            this.exportServiceApi = exportServiceApi;
            this.exportOptions = exportOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = this.exportOptions.Value.ExportServiceUrl;

            try
            {
                var status = await this.exportServiceApi.GetConnectivityStatus();
                return HealthCheckResult.Healthy(Diagnostics.export_service_connectivity_check_Healthy.FormatString(uri, status));
            }
            catch (ApiException apiException)
            {
                return HealthCheckResult.Unhealthy(
                    Diagnostics.export_service_connectivity_check_Unhealthy.FormatString(uri, apiException.Content));
            }
            catch (Exception e)
            {
                return HealthCheckResult.Degraded(
                    Diagnostics.export_service_connectivity_check_Degraded.FormatString(uri), e);
            }
        }
    }
}
