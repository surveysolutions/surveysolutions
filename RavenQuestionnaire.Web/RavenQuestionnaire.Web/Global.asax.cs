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
    using System.Net;
    using System.Net.Sockets;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using NConfig;

    using NLog;

    using Questionnaire.Core.Web.Helpers;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Per AppDomain initialization.
        /// </summary>
        static MvcApplication()
        {
            SetupNConfig();
        }

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
               "Api",
                // Route name
               "api/{action}/{id}",
                // URL with parameters
               new { controller = "WillBeApi", id = UrlParameter.Optional } // Parameter defaults
               );

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
                SuccessMarker.Start();
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
                base.Response.Write("<html>");

                base.Response.Write("Sorry, Application can't handle this!");

                string errorDescription = GetUserFriendlyErrorDescription(intializationException);
                if (errorDescription != null)
                {
                    base.Response.Write(string.Format("<br/><br/><i>{0}</i>", errorDescription));
                }

                base.Response.Write("</html>");

                this.CompleteRequest();
            }
        }

        private static string GetUserFriendlyErrorDescription(Exception exception)
        {
            bool isRavenNotAvailable =
                exception is WebException &&
                exception.InnerException is SocketException &&
                ((SocketException)exception.InnerException).ErrorCode == 10061;

            if (isRavenNotAvailable)
                return "Seems like RavenDB is not available. Please, check that RavenDB server is running and that web.congig contains corrent post and host.";

            bool isRavenPostForbidden =
                exception.InnerException is WebException &&
                ((WebException)exception.InnerException).Response is HttpWebResponse &&
                ((HttpWebResponse)((WebException)exception.InnerException).Response).Method == "POST" &&
                ((HttpWebResponse)((WebException)exception.InnerException).Response).StatusCode == HttpStatusCode.Forbidden;

            if (isRavenPostForbidden)
                return "RavenDB forbids POST requests. You can allow them by setting Raven/AnonymousAccess flag to All in RavenDB server config file. See wiki for more details.";

            return "Please see log file for details or (better) debug the application.";
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

        private static void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Designer.Web.config").SetAsSystemDefault();
        }

        #endregion
    }
}