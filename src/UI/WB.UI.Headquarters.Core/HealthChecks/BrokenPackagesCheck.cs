using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.HealthChecks
{
    public class BrokenPackagesCheck : IHealthCheck
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly LinkGenerator linkGenerator;

        public BrokenPackagesCheck(IHttpContextAccessor contextAccessor, LinkGenerator linkGenerator)
        {
            this.contextAccessor = contextAccessor;
            this.linkGenerator = linkGenerator;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var unexpected = DatabaseStatsCollector.BrokenPackagesCount.Labels("Unexpected").Value;

            if (unexpected > 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(Diagnostics.UnexpectedBrokenPackages.FormatString(unexpected),
                        data: new Dictionary<string, object>
                        {
                            ["url"] = linkGenerator.GetPathByAction(this.contextAccessor.HttpContext,
                                "InterviewPackages", "Diagnostics")
                        }));
            }

            return Task.FromResult(HealthCheckResult.Healthy("There is no Unexpected packages detected"));
        }
    }
}
