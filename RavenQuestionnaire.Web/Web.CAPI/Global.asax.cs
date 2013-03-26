// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="">
//   
// </copyright>
// <summary>
//   The mvc application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.CAPI
{
    using System;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using System.Web.Routing;

    using NLog;

    using Questionnaire.Core.Web.Helpers;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The MVC application.
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
            // filters.Add(new HandleErrorAttribute());
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
                //// Route name
                "{controller}/{action}/{id}", 
                //// URL with parameters
                new { controller = "Survey", action = "Dashboard", id = UrlParameter.Optional } //// Parameter defaults
                );
        }

        #endregion

        #region Methods

        /// <summary>
        /// The application_ error.
        /// </summary>
        protected void Application_Error()
        {
            var lastError = this.Server.GetLastError();
            this.logger.Fatal(lastError);
            if (lastError.InnerException != null)
                this.logger.Fatal(lastError.InnerException);
        }

        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            try
            {
                SuccessMarker.Start();
                correctlyInitialyzed = true;
            }
            catch (Exception e)
            {
                this.logger.Fatal("Initialization failed", e);
                correctlyInitialyzed = false;

                // due to the bug in iis7 moved to Application_BeginRequest
                /*this.BeginRequest += (sender, args) =>
                {
                    base.Response.Write("Sorry, Application cann't handle your request!");
                    this.CompleteRequest();
                };*/

                // throw;
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!correctlyInitialyzed)
            {
                base.Response.Write("Sorry, Application cann't handle your request. Initialization of storage was not correct.");
                this.CompleteRequest();
            }
        }

        /// <summary>
        /// The correctly initialyzed.
        /// </summary>
        private static bool correctlyInitialyzed;

        /// <summary>
        /// The host services.
        /// </summary>
        protected void HostServices()
        {
            bool isDiscovereble;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["WCFVisible"], out isDiscovereble))
            {
                return;
            }

            if (!isDiscovereble)
            {
                return;
            }

            // i need to ping wcf server to make it visible or install app fabric
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