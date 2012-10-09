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
    using System.Web.Configuration;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Main.Core;

    using Ninject;

    using NLog;

    using Questionnaire.Core.Web.Helpers;

    using Raven.Client.Document;

    using RavenQuestionnaire.Web.Controllers;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
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
                // Route name
                "{controller}/{action}/{id}", 
                // URL with parameters
                new { controller = "Questionnaire", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        #endregion

        #region Methods

        /// <summary>
        /// The application_ error.
        /// </summary>
        protected void Application_Error()
        {
            Exception lastException = this.Server.GetLastError();
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);

            Exception exception = this.Server.GetLastError();

            this.Response.Clear();

            var httpException = exception as HttpException;

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            if (httpException == null)
            {
                routeData.Values.Add("action", "Index");
            }
            else
            {
                // It's an Http Exception, Let's handle it.
                switch (httpException.GetHttpCode())
                {
                    case 403:
                        // No access
                        routeData.Values.Add("action", "Http403");
                        break;
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "Http404");
                        break;
                    case 500:
                        // Server error.
                        routeData.Values.Add("action", "Http500");
                        break;
                    default:
                        routeData.Values.Add("action", "General");
                        break;
                }
            }

            // Pass exception details to the target error View.
            routeData.Values.Add("error", exception);

            // Clear the error on server.
            this.Server.ClearError();

            // Avoid IIS7 getting in the middle
            this.Response.TrySkipIisCustomErrors = true;

            // Call target Controller and pass the routeData.
            IController errorController = new ErrorController();
            errorController.Execute(new RequestContext(new HttpContextWrapper(this.Context), routeData));
        }

        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // maybe better to move outside this class
            NCQRSInit.RebuildReadLayer(KernelLocator.Kernel.Get<DocumentStore>());
        }

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

        #endregion
    }
}