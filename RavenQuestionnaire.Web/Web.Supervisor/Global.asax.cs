using NConfig;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using WB.Core.SharedKernel.Utils.Logging;

    using Web.Supervisor.App_Start;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Initialization per AppDomain.
        /// </summary>
        static MvcApplication()
        {
            SetupNConfig();
        }

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(MvcApplication));

        /// <summary>
        /// The correctly initialized.
        /// </summary>
        private static bool correctlyInitialized;

        /// <summary>
        /// The register global filters.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        /// <summary>
        /// The register routes.
        /// </summary>
        /// <param name="routes">
        /// The routes.
        /// </param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                "Default", 
                "{controller}/{action}/{id}", 
                new { controller = "Survey", action = "Index", id = UrlParameter.Optional } 
            );
        }

        /// <summary>
        /// The application_ error.
        /// </summary>
        protected void Application_Error()
        {
            Exception lastError = this.Server.GetLastError();
            this.logger.Fatal(lastError);
            if (lastError.InnerException != null)
            {
                this.logger.Fatal(lastError.InnerException);
            }
        }

        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            this.logger.Info("Starting application.");

            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // RouteTable.Routes.Add(new ServiceRoute("", new Ninject.Extensions.Wcf.NinjectServiceHostFactory(), typeof(API)));

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

        /// <summary>
        /// The current_ unhandled exception.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CurrentUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exp = (Exception)e.ExceptionObject;
                this.logger.Fatal("Global Unhandled:", exp);
                //this.logger.Fatal(e.ExceptionObject);
            }
            catch (Exception)
            {
                //throw;
            }
            
        }

        private static void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Supervisor.Web.config").SetAsSystemDefault();
        }

        #warning TLK: delete this when NCQRS initialization moved to Global.asax
        public static void Initialize() { }
    }
}