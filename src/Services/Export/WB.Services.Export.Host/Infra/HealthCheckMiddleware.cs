using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.Checks;

namespace WB.Services.Export.Host.Infra
{
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string path;

        public HealthCheckMiddleware(RequestDelegate next, string path)
        {
            this.next = next;
            this.path = path;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.ToLowerInvariant() == path)
            {
                var checks = context.RequestServices.GetServices<IHealthCheck>();

                foreach (var check in checks)
                {
                    var sw = Stopwatch.StartNew();
                    bool isOk = false;
                    try
                    {
                        isOk = await check.CheckAsync();
                    }
                    catch { /* om om om */}
                    finally
                    {
                        await context.Response.WriteAsync($"{(isOk ? "OK" : "FAIL")}: {check.Name}. Took: {sw.ElapsedMilliseconds}ms\r\n");
                    }
                }

            }
            else
            {
                await next(context);
            }
        }
    }

    public static class HealthCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder builder, string path)
        {
            return builder.UseMiddleware<HealthCheckMiddleware>(path);
        }

        public static IServiceCollection ConfigureHealthCheck<T>(this IServiceCollection services) where T : class, IHealthCheck
        {
            services.AddTransient<IHealthCheck, T>();
            return services;
        }
    }
}
