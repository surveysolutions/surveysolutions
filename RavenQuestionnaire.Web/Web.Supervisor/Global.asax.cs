// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="">
//   
// </copyright>
// <summary>
//   The mvc application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using NLog;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods and Operators

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
                // Route name
                "{controller}/{action}/{id}", 
                // URL with parameters
                new { controller = "Survey", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        #endregion

        #region Methods

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
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;
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
            this.logger.Fatal(e.ExceptionObject);
        }

        #endregion
    }
}