using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;

namespace WB.UI.Headquarters
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(@"{resource}.axd/{*pathInfo}");

            routes.MapRoute(@"WebInterview.Start", @"WebInterview/Start/{id}", new { controller = @"WebInterview", action = @"Start" });
            routes.MapRoute(@"WebInterview.Resume", @"WebInterview/{id}/Section/{sectionId}", 
                defaults: new { controller = @"WebInterview", action = @"Section" });
            routes.MapRoute(@"WebInterview.ImageAnswering", @"WebInterview/image", new { controller = @"WebInterview", action = @"Image" });

            routes.MapRoute(@"WebInterview", @"WebInterview/{id}/Cover",
                defaults: new { controller = @"WebInterview", action = @"Cover" });

            routes.MapRoute(@"WebInterview.Complete", @"WebInterview/{id}/Complete",
                defaults: new { controller = @"WebInterview", action = @"Complete" },
                constraints: new { id = new GuidRouteConstraint() });

            routes.MapRoute(@"Default", @"{controller}/{action}/{id}",
                new { controller = @"Account", action = @"Index", id = UrlParameter.Optional });
        }
    }
}