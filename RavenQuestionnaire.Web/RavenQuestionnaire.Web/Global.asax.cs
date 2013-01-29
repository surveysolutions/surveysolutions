// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="">
//   
// </copyright>
// <summary>
//   The mvc application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Web
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using NLog;

    using Questionnaire.Core.Web.Helpers;

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

        /// <summary>
        /// The correctly initialyzed.
        /// </summary>
        private static bool correctlyInitialyzed;

        private static Exception intializationException;

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
                new { controller = "Questionnaire", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        #endregion

        /* /// <summary>
        /// The application_ error.
        /// </summary>
        protected void Application_Error()
        {
            Exception lastException = this.Server.GetLastError();
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }*/
        #region Methods

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

            try
            {
                SuccessMarker.Start(KernelLocator.Kernel);
                correctlyInitialyzed = true;
            }
            catch (Exception e)
            {
                this.logger.Fatal("Initialization failed", e);
                correctlyInitialyzed = false;
                intializationException = e;

                // due to the bug in iis7 moved to Application_BeginRequest
                /*this.BeginRequest += (sender, args) =>
                    {
                        base.Response.Write("Sorry, Application cann't handle his!");
                        this.CompleteRequest();
                    };
                throw;*/
            }

            // maybe better to move outside this class
            // NCQRSInit.RebuildReadLayer(KernelLocator.Kernel.Get<DocumentStore>());
            // RegisterGlobalFilters(GlobalFilters.Filters);
            // RegisterRoutes(RouteTable.Routes);
            // ViewEngines.Engines.Clear();
            // ViewEngines.Engines.Add(new RazorViewEngine());
            // ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!correctlyInitialyzed)
            {
                base.Response.Write("Sorry, Application can't handle this!<br/>");
                base.Response.Write(intializationException.ToString().Replace(Environment.NewLine, "<br/>"));
                this.CompleteRequest();
            }
        }

        // <summary>
        /// <summary>
        /// The current unhandled exception.
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