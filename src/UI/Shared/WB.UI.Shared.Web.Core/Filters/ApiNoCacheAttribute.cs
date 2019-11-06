using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiNoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            if (filterContext.Response?.Headers == null) return;

            filterContext.Response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
        }
    }
}