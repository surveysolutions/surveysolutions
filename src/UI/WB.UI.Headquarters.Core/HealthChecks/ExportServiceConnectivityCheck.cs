﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class ExportServiceConnectivityCheck : IHealthCheck
    {
        private readonly IInScopeExecutor scope;
        private readonly IOptions<DataExportOptions> exportOptions;

        public ExportServiceConnectivityCheck(
            IOptions<DataExportOptions> exportOptions, IInScopeExecutor scope)
        {
            this.exportOptions = exportOptions;
            this.scope = scope;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = this.exportOptions.Value.ExportServiceUrl;

            try
            {
                return await scope.ExecuteAsync(async sl =>
                {
                    var api = sl.GetInstance<IExportServiceApi>();
                    var status = await api.GetConnectivityStatus();

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        status = "OK";
                    }

                    return HealthCheckResult.Healthy(
                        Diagnostics.export_service_connectivity_check_Healthy
                            .FormatString(uri, status));
                });
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
                    Diagnostics.export_service_connectivity_check_Degraded.FormatString(uri), e);
            }
        }
    }
}
