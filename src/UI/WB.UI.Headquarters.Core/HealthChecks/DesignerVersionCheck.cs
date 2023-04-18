using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class DesignerVersionCheck : IHealthCheck
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly IDesignerApi designerApi;

        public DesignerVersionCheck(
            IRestServiceSettings restServiceSettings, IDesignerApi designerApi)
        {
            this.restServiceSettings = restServiceSettings;
            this.designerApi = designerApi;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var version = await designerApi.Version();
                return HealthCheckResult.Healthy(Diagnostics.designer_service_check_Healthy.FormatString(restServiceSettings.Endpoint, version));
            }
            catch (Exception e)
            {
                return HealthCheckResult.Degraded(Diagnostics.designer_connectivity_check_Degraded.FormatString(restServiceSettings.Endpoint), 
                    exception: new Exception(e.Message));
            }
        }
    }
}
