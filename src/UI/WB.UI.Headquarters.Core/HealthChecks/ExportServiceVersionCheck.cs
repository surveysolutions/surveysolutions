using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class ExportServiceVersionCheck : IHealthCheck
    {
        private readonly IOptionsSnapshot<ExportServiceConfig> exportOptions;
        private readonly IInScopeExecutor<IExportServiceApi> scope;

        public ExportServiceVersionCheck(
            IOptionsSnapshot<ExportServiceConfig> exportOptions, IInScopeExecutor<IExportServiceApi> scope)
        {
            this.exportOptions = exportOptions;
            this.scope = scope;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var uri = this.exportOptions.Value.ExportServiceUrl + "/.version";
            try
            {
                return await scope.ExecuteAsync(async api =>
                {
                    var version = await api.Version();
                    return HealthCheckResult.Healthy(Diagnostics.export_service_check_Healthy.FormatString(uri, version));
                }, WorkspaceConstants.DefaultWorkspaceName);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Degraded(Diagnostics.export_service_connectivity_check_Degraded.FormatString(uri), e);
            }
        }
    }
}
