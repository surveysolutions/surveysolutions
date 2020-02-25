using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;

namespace WB.UI.Headquarters.HealthChecks
{
    public class ExportServiceCheck : IHealthCheck
    {
        private readonly IOptions<DataExportOptions> exportOptions;

        public ExportServiceCheck(IOptions<DataExportOptions> exportOptions)
        {
            this.exportOptions = exportOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = this.exportOptions.Value.ExportServiceUrl + "/.version";
            try
            {
                var version = await new HttpClient { Timeout = TimeSpan.FromSeconds(2) }.GetStringAsync(uri);
                return HealthCheckResult.Healthy($"Export service at {uri} responded with 'v{version}' ");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Degraded($"No connection to Export Service at {uri}", e);
            }
        }
    }
}
