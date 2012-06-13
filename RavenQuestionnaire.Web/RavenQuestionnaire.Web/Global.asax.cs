using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using DataEntryWCFServer;
using SynchronizationMessages.Discover;

namespace RavenQuestionnaire.Web
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
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            HostServices();
        }
        protected void HostServices()
        {
            bool isDiscovereble;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["WCFVisible"], out  isDiscovereble))
                return;
            if(!isDiscovereble)
                return;
            string hostname = System.Net.Dns.GetHostName();
            var baseAddress = new UriBuilder("http", hostname, 80, "SpotSyncService");
            var h = new ServiceHost(typeof(SpotSyncService), baseAddress.Uri);
            // enable processing of discovery messages.  use UdpDiscoveryEndpoint to enable listening. use EndpointDiscoveryBehavior for fine control.
            h.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            h.AddServiceEndpoint(new UdpDiscoveryEndpoint());


            // create endpoint
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            h.AddServiceEndpoint(typeof(ISpotSync), binding, "");
            h.Open();
        }
        protected void Application_Error()
        {
            Exception lastException = Server.GetLastError();
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }
    }
}