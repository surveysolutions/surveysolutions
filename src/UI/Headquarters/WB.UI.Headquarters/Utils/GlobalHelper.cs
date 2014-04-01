using System.Web;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Utils
{
    public static class GlobalHelper
    {
        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return url.Action(action, controller, routes, HttpContext.Current.Request.Url.Scheme);
        } 
        
    }
}