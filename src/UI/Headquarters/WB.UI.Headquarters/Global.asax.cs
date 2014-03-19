using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NConfig;
using WB.UI.Headquarters.App_Start;

namespace WB.UI.Headquarters
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static MvcApplication()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Used to trigger execution of static constructor
        /// </summary>
        public static void Initialize()
        {
        }
    }
}