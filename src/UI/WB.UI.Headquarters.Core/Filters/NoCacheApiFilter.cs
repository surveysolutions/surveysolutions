using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace WB.UI.Headquarters.Filters
{
    public class ExtraHeadersApiFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {

            context.HttpContext.Response.Headers.Add("X-Xss-Protection", "1");
            context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            context.HttpContext.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");

            if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.HttpContext.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue()
                    {
                        NoStore = true,
                        NoCache = true,
                    };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
