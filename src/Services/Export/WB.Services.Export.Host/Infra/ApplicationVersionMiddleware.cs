using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WB.Services.Export.Host.Infra
{
    public class ApplicationVersionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string path;
        private readonly string productVersion;

        public ApplicationVersionMiddleware(RequestDelegate next, string path)
        {
            this.next = next;
            this.path = path.ToLowerInvariant();

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            productVersion = fvi.FileVersion;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.ToLowerInvariant() == path)
            {

                context.Response.StatusCode = 200;
                context.Response.ContentLength = productVersion.Length;

                await context.Response.WriteAsync(productVersion);

            }
            else
            {
                await this.next(context);
            }
        }
    }
}
