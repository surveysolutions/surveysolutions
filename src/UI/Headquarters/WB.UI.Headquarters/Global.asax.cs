using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Elmah;
using Microsoft.Practices.ServiceLocation;
using NConfig;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Shared.Web.Elmah;

namespace WB.UI.Headquarters
{
    public class Global : HttpApplication
    {
         /// <summary>
        /// Initialization per AppDomain.
        /// </summary>
        static Global()
        {
            SetupNConfig();
        }

        private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                "Default", 
                "{controller}/{action}/{id}", 
                new { controller = "Survey", action = "Index", id = UrlParameter.Optional } 
            );
        }

        protected void Application_Error()
        {
            Exception lastError = this.Server.GetLastError();
            this.logger.Fatal("Unexpected error occurred", lastError);
            if (lastError.InnerException != null)
            {
                this.logger.Fatal("Unexpected error occurred", lastError.InnerException);
            }
        }

        protected void Application_Start()
        {
            this.logger.Info("Starting application.");

            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);

            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // RouteTable.Routes.Add(new ServiceRoute("", new Ninject.Extensions.Wcf.NinjectServiceHostFactory(), typeof(API)));

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(SetupViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

        private static RazorViewEngine SetupViewEngine()
        {
            var viewEngine = new RazorViewEngine();

            string[] engineViewPath =
            {
                "~/bin/Views/{1}/{0}.cshtml",
                "~/bin/Views/Shared/{0}.cshtml"
            };

            viewEngine.AreaMasterLocationFormats = viewEngine.AreaMasterLocationFormats.Union(engineViewPath).ToArray();
            viewEngine.AreaViewLocationFormats = viewEngine.AreaViewLocationFormats.Union(engineViewPath).ToArray();
            viewEngine.AreaPartialViewLocationFormats = viewEngine.AreaPartialViewLocationFormats.Union(engineViewPath).ToArray();

            return viewEngine;
        }

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
            NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();
        }

        #warning TLK: delete this when NCQRS initialization moved to Global.asax
        public static void Initialize() { }

        public override void Init()
        {
            this.PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
            base.Init();
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext.Current.SetSessionStateBehavior(
                SessionStateBehavior.Required);
        }

        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            var ctx = e.Context as HttpContext;
            if (ctx == null)
            {
                return;
            }
            ElmahDataFilter.Apply(e, ctx);
        }
    }
}