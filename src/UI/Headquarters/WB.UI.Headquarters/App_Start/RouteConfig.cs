using System.Web.Mvc;
using System.Web.Routing;

namespace WB.UI.Headquarters
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "InterviewRoute",
                "Interview/{action}/{id}",
                new { controller = "Interview", id = UrlParameter.Optional });

            routes.MapRoute(
                "BackupRoute",
                "Backup/{action}/{id}",
                new { controller = "Backup", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                "ImportExportRoute",
                "ImportExport/{action}/{id}",
                new { controller = "ImportExport", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                "SurveyRoute",
                "Survey/{action}/{id}",
                new { controller = "Survey", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                "TabletReportRoute",
                "TabletReport/{action}/{id}",
                new { controller = "TabletReport", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
        }
    }
}