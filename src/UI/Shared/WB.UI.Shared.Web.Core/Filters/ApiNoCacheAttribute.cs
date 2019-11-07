using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiNoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.Response?.Headers == null) return;

            context.Response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
        }
    }
}
