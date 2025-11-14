using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class HeadquartersUrlBindingCheck : IHealthCheck
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IOptions<HeadquartersConfig> hqConfig;

        public HeadquartersUrlBindingCheck(IHttpContextAccessor contextAccessor, IOptions<HeadquartersConfig> hqConfig)
        {
            this.contextAccessor = contextAccessor;
            this.hqConfig = hqConfig;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var baseUrl = this.hqConfig.Value.BaseUrl;
            var requestUrl = this.contextAccessor.HttpContext.Request.GetDisplayUrl();

            if (requestUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    Diagnostics.hq_baseurl_check_Healthy.FormatString(baseUrl)));
            }

            return Task.FromResult(HealthCheckResult.Degraded(
                Diagnostics.hq_baseurl_check_Degraded.FormatString(baseUrl)));
        }
    }
}
