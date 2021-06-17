using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace WB.UI.Headquarters.Filters
{
    public class ExtraHeadersApiFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {         
            if (context.HttpContext.Request.Path.StartsWithSegments("/graphql"))
            { 
            }
            else if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.HttpContext.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue()
                    {
                        NoStore = true,
                        NoCache = true,
                    };
            }
            else
            {
                AddOrUpdateHeader(context,"X-Xss-Protection", "1");
                AddOrUpdateHeader(context,"X-Content-Type-Options", "nosniff");                
                AddOrUpdateHeader(context,"X-Frame-Options", "SAMEORIGIN");
                AddOrUpdateHeader(context,"Content-Security-Policy", "font-src 'self' data:; img-src 'self' data:; default-src 'self' 'unsafe-inline' 'unsafe-eval'");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        private void AddOrUpdateHeader(ActionExecutingContext context, string key, string value)
        { 
            if(context.HttpContext.Response.Headers.ContainsKey(key))
                    context.HttpContext.Response.Headers.Remove(key);
                context.HttpContext.Response.Headers.Add(key, value);
        }
    }
}
