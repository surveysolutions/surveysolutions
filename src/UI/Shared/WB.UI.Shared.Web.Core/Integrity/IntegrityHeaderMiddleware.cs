using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.HttpServices.Services;

namespace WB.UI.Shared.Web.Integrity;

public class IntegrityHeaderMiddleware
{
    private readonly RequestDelegate next;
    private readonly string headerValue;
    private readonly ILogger<IntegrityHeaderMiddleware> logger;

    public IntegrityHeaderMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<IntegrityHeaderMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
        this.headerValue = configuration["Integrity:HeaderValue"] ?? IntegrityService.IntegrityHeaderValue;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!string.IsNullOrEmpty(headerValue))
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(IntegrityService.IntegrityHeaderName))
                    context.Response.Headers[IntegrityService.IntegrityHeaderName] = headerValue;
                return Task.CompletedTask;
            });
        }

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted || !IsApiRequest(context))
                throw;

            // This fallback is intentionally limited to API/XHR requests so MVC/non-API requests
            // keep their standard exception flow and error pages.
            // Log at Debug to avoid duplicating the ERROR entry already emitted by UseExceptional
            // (Headquarters, Designer) or captured by UseSerilogRequestLogging (WebTester).
            logger.LogDebug(ex, "Unhandled exception for API request {Path}. Returning 500 with integrity header.",
                context.Request.Path);

            try
            {
                context.Response.Clear();
            }
            catch (InvalidOperationException)
            {
                // Response.Clear() failed — cannot write fallback response.
                // Re-throw the original exception to preserve its stack trace.
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            if (!string.IsNullOrEmpty(headerValue))
                context.Response.Headers[IntegrityService.IntegrityHeaderName] = headerValue;
            await context.Response.CompleteAsync();
        }
    }

    private static bool IsApiRequest(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/api") || path.StartsWithSegments("/graphql"))
            return true;

        return string.Equals(context.Request.Headers["X-Requested-With"],
            "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }
}
