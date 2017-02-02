using System.Web.Mvc;
using System.Web.Routing;

namespace WB.UI.Headquarters
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(@"{resource}.axd/{*pathInfo}");

            routes.MapRoute(@"WebInterviewStart", @"WebInterview/Start/{id}", new { controller = @"WebInterview", action = @"Start" });
            routes.MapRoute(@"WebInterview.ImageAnswering", @"WebInterview/image", new { controller = @"WebInterview", action = @"Image" });
            routes.MapRoute(@"WebInterview", @"WebInterview/{*path}", new { controller = @"WebInterview", action = @"Index" });

            routes.MapRoute(@"Default", @"{controller}/{action}/{id}",
                new { controller = @"Account", action = @"Index", id = UrlParameter.Optional });
        }
    }
}