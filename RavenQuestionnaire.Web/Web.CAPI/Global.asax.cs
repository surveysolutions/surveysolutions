using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using RavenQuestionnaire.Web.App_Start;

namespace Web.CAPI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Survey", action = "Dashboard", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            NCQRSInit.RebuildReadLayer();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
        }
        protected void HostServices()
        {
            bool isDiscovereble;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["WCFVisible"], out isDiscovereble))
                return;
            if (!isDiscovereble)
                return;
            // i need to ping wcf server to make it visible or install app fabric
        }

        protected void Application_Error()
        {
            Exception lastException = Server.GetLastError();
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }
    }
}