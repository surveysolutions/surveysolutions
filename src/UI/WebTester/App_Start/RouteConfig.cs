using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;

namespace WB.UI.WebTester
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Section", "WebTester/Interview/{id}/Section/{sectionId}", 
                defaults: new { controller = "WebTester", action = "Section" },
                constraints: new { id = new GuidRouteConstraint() });

            routes.MapRoute("Interview", "WebTester/Interview/{id}/{*url}", new
            {
                controller = "WebTester",
                action = "Interview"
            });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "WebTester", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
