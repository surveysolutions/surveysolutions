using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace WB.UI.Headquarters.Filters
{
    public class NoCacheApiFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
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