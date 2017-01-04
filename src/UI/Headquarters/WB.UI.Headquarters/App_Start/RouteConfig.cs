using System.Web.Mvc;
using System.Web.Routing;

namespace WB.UI.Headquarters
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(@"{resource}.axd/{*pathInfo}");

            routes.MapRoute("WebInterview", @"webinterview/{*path}", new { controller = "WebInterview", action = @"Index" });

            routes.MapRoute(@"Default", @"{controller}/{action}/{id}",
                new { controller = @"Account", action = @"Index", id = UrlParameter.Optional });
        }
    }
}