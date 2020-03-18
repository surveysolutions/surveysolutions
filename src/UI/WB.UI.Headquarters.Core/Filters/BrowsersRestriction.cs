using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UAParser;

namespace WB.UI.Headquarters.Filters
{
    public class BrowsersRestrictionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ActionDescriptor.DisplayName.Contains("OutdatedBrowser") && filterContext.HttpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                string userAgentString = filterContext.HttpContext.Request.Headers["User-Agent"].ToString();
                var parser = Parser.GetDefault();
                var userAgent = parser.ParseUserAgent(userAgentString);
                if (IsInternetExplorer(userAgent) && IsAllowGetMajorVersion(userAgent))
                {
                    filterContext.Result = new RedirectToActionResult("OutdatedBrowser", "WebInterview", new{});
                }
            }
        }

        private bool IsAllowGetMajorVersion(UserAgent userAgent)
        {
            if (int.TryParse(userAgent.Major, out int version))
                return version < 10;
            return true;
        }

        public static bool IsInternetExplorer(UserAgent userAgent)
        {
            if (userAgent.Family.Contains("MSIE") || userAgent.Family.Contains("Trident"))
            {
                return true;
            }
            return false;
        }
    }
}
