using Microsoft.AspNetCore.Builder;

namespace WB.Services.Export.Host.Infra
{
    public static class ApplicationVersionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApplicationVersion(this IApplicationBuilder builder, string path)
        {
            return builder.UseMiddleware<ApplicationVersionMiddleware>(path);
        }
    }
}