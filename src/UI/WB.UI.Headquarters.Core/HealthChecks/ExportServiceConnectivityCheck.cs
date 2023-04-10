using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class ExportServiceConnectivityCheck : IHealthCheck
    {
        private readonly IInScopeExecutor<IExportServiceApi> scope;
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;

        public ExportServiceConnectivityCheck(
            IOptionsSnapshot<ExportServiceConfig> exportOptions, IInScopeExecutor<IExportServiceApi> scope)
        {
            this.exportOptions = exportOptions;
            this.scope = scope;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = this.exportOptions.Value.ExportServiceUrl;

            try
            {
                return await scope.ExecuteAsync(async api=>
                {
                    var status = await api.GetConnectivityStatus();

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        status = "OK";
                    }

                    return HealthCheckResult.Healthy(
                        Diagnostics.export_service_connectivity_check_Healthy
                            .FormatString(uri, status));
                }, WorkspaceConstants.DefaultWorkspaceName);
            }
            catch (ApiException apiException)
            {
                return HealthCheckResult.Unhealthy(
                    Diagnostics.export_service_connectivity_check_Unhealthy.FormatString(uri,
                        apiException.Content));
            }
            catch (Exception e)
            {
                return HealthCheckResult.Degraded(
                    Diagnostics.export_service_connectivity_check_Degraded.FormatString(uri), 
                    data: new Dictionary<string, object>()
                    {
                        ["Message"] = e.Message
                    });
            }
        }
    }
}
