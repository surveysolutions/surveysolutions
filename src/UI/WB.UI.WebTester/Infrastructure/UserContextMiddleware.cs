using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Enriches the Serilog/ILogger scope for every request with UserId, CorrelationId,
    /// TraceId and ServiceName, sourced from <see cref="IUserContextStore"/>.
    /// Must be placed AFTER UseRouting() so that route values are available.
    /// The interview ID is read from <see cref="DesignerJwtContext.InterviewId"/> which is
    /// set by <see cref="WebTesterSessionAuthorizeAttribute"/> earlier in the filter pipeline.
    /// For unauthenticated requests (e.g., Run before session is established) the scope
    /// fields will be "unknown" / "none".
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
                // DesignerJwtContext.InterviewId is set by WebTesterSessionAuthorizeAttribute
                // (ActionFilter), which runs after this middleware.  However, for background
                // import tasks the value is already present in the AsyncLocal captured by Run().
                // For ordinary HTTP requests the value will be null here (filters haven't run yet)
                // so we fall back to extracting the route 'id' directly.
                var interviewId = DesignerJwtContext.InterviewId;

                if (interviewId == null)
                {
                    if (httpContext.Request.RouteValues.TryGetValue("id", out var idValue)
                            && Guid.TryParse(idValue as string ?? idValue?.ToString(), out var routeId))
                        interviewId = routeId;
                }

                if (interviewId.HasValue)
                    ctx = userContextStore.Get(interviewId.Value);
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
