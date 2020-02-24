using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Diagnostics
{
    public static class DiagnosticsExtensions
    {
        private static string version = null;
        public static void MapVersionEndpoint(this IEndpointRouteBuilder endpoints, string pattern = ".version")
        {
            endpoints.MapGet("/" + pattern, context =>
            {
                version ??= context.RequestServices.GetRequiredService<IProductVersion>().ToString();
                return context.Response.WriteAsync(version);
            });
        }
    }
}
