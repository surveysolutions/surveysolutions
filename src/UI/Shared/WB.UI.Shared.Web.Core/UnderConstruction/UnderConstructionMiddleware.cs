using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.UnderConstruction
{
    public class UnderConstructionMiddleware
    {
        private readonly RequestDelegate next;
        
        public UnderConstructionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/UnderConstruction")
            && !context.Request.Path.StartsWithSegments("/.hc"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.Headers.Add("Retry-After", "30");
                context.Request.Path = "/UnderConstruction";
            }

            return next.Invoke(context);
        }

        public static bool When(HttpContext ctx)
        {
            UnderConstructionStatus CurrentStatus(IServiceProvider provider)
            {
                return provider.GetRequiredService<UnderConstructionInfo>().Status;
            }

            return CurrentStatus(ctx.RequestServices) != UnderConstructionStatus.Finished;
        }
    }
}
