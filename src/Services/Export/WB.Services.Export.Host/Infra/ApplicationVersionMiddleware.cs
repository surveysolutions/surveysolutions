using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WB.Services.Export.Host.Infra
{
    public class ApplicationVersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;
        private string _productVersion;

        public ApplicationVersionMiddleware(RequestDelegate next, string path)
        {
            _next = next;
            _path = path.ToLowerInvariant();

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            _productVersion = fvi.ProductVersion;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.ToLowerInvariant() == _path)
            {

                context.Response.StatusCode = 200;
                context.Response.ContentLength = _productVersion.Length;

                await context.Response.WriteAsync(_productVersion);

            }
            else
            {
                await this._next(context);
            }
        }
    }
}
