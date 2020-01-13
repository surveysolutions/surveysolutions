using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class BrowsersRestrictionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (filterContext.ActionDescriptor.DisplayName != "OutdatedBrowser")
            //{
            //    var httpContextRequest = filterContext.HttpContext.Request;
            //    if (httpContextRequest.Browser.Browser == "IE" && httpContextRequest.Browser.MajorVersion < 10)
            //    {
            //        var routeValueDictionary = new RouteValueDictionary(new
            //        {
            //            controller = "WebInterview",
            //            action = "OutdatedBrowser"
            //        });
            //        filterContext.Result = new RedirectToRouteResult(routeValueDictionary);
            //    }
            //}
        }
    }
}
