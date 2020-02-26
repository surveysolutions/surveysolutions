﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;

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
            var unexpected = BrokenPackagesStatsCollector.BrokenPackagesCount.Labels("Unexpected").Value;

            if (unexpected > 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded($"There is {unexpected} Unexpected broken packages detected", data: new Dictionary<string, object>
                    {
                        ["url"] = linkGenerator.GetPathByAction(this.contextAccessor.HttpContext, 
                            "InterviewPackages", "ControlPanel")
                    }));
            }

            return Task.FromResult(HealthCheckResult.Healthy("There is no Unexpected packages detected"));
        }
    }
}