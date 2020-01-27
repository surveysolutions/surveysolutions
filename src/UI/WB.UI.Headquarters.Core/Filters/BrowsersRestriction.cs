using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NLog.Web.LayoutRenderers;

namespace WB.UI.Headquarters.Filters
{
    public class BrowsersRestrictionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.DisplayName != "OutdatedBrowser" && filterContext.HttpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                string userAgent = filterContext.HttpContext.Request.Headers["User-Agent"].ToString();
                if (IsInternetExplorer(userAgent) && GetMajorVersion(userAgent) < 10)
                {
                    var routeValueDictionary = new RouteValueDictionary(new
                    {
                        controller = "WebInterview",
                        action = "OutdatedBrowser"
                    });
                    filterContext.Result = new RedirectToRouteResult(routeValueDictionary);
                }
            }
        }

        private int GetMajorVersion(string userAgent)
        {
            throw new ArgumentException("Need parse user-agent");
            //return httpContextRequest.Browser.MajorVersion < 10
        }

        public static bool IsInternetExplorer(string userAgent)
        {
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
