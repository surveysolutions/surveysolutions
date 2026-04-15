using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Enriches the Serilog/ILogger scope for every request with UserId, CorrelationId,
    /// TraceId and ServiceName, sourced from <see cref="IUserContextStore"/>.
    /// Must be placed AFTER UseRouting() so that route values are available.
    /// </summary>
    public class UserContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<UserContextMiddleware> logger;

        public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var userContextStore = httpContext.RequestServices.GetService<IUserContextStore>();
            RequestUserContext? ctx = null;

            if (userContextStore != null)
            {
                // Try to extract questionnaire/interview id from route data
                var routeData = httpContext.GetRouteData();
                if (routeData?.Values.TryGetValue("id", out var idValue) == true
                    && Guid.TryParse(idValue?.ToString(), out var questionnaireId))
                {
                    ctx = userContextStore.Get(questionnaireId);
                }
            }

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["UserId"]        = ctx?.UserId ?? "unknown",
                ["CorrelationId"] = ctx?.CorrelationId ?? "none",
                ["TraceId"]       = httpContext.TraceIdentifier,
                ["ServiceName"]   = "WB.WebTester"
            }))
            {
                await next(httpContext);
            }
        }
    }
}
