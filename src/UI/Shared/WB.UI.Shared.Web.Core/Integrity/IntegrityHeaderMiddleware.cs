using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WB.Core.Infrastructure.HttpServices.Services;

namespace WB.UI.Shared.Web.Integrity;

public class IntegrityHeaderMiddleware
{
    private readonly RequestDelegate next;
    private readonly string headerValue;

    public IntegrityHeaderMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        this.headerValue = configuration["Integrity:HeaderValue"] ?? IntegrityService.IntegrityHeaderValue;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!string.IsNullOrEmpty(headerValue))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[IntegrityService.IntegrityHeaderName] = headerValue;
                return Task.CompletedTask;
            });
        }

        await next(context);
    }
}
