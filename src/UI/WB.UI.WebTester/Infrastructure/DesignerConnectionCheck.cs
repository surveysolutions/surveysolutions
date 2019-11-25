using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class DesignerConnectionCheck : IHealthCheck
    {
        private readonly IDesignerWebTesterApi testerApi;

        public DesignerConnectionCheck(IDesignerWebTesterApi testerApi)
        {
            this.testerApi = testerApi;
        }

        const string Description = "Connection to Designer App";

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await testerApi.HealthCheck();
                if (result == HealthStatus.Healthy.ToString())
                {
                    return HealthCheckResult.Healthy(Description);
                }

                return HealthCheckResult.Unhealthy(Description, data: new Dictionary<string, object>
                {
                    ["Status"] = result
                });
            }
            catch (Exception e){
                return HealthCheckResult.Unhealthy(Description, e);
            }
            
        }
    }
}
