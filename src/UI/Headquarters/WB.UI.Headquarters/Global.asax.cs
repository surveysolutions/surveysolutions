﻿using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Elmah;
using EmbeddedResourceVirtualPathProvider;
using Microsoft.Practices.ServiceLocation;
using NConfig;

using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Elmah;
using WB.UI.Shared.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;

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
        private readonly IHealthCheckService healthCheckService = ServiceLocator.Current.GetInstance<IHealthCheckService>();

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ReplacePrincipal());
            filters.Add(new RequireSecureConnectionAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MaintenanceFilter());
            //filters.Add(new SupervisorFunctionsEnabledAttribute());
            filters.Add(new InstallationAttribute(), 100);
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new ReplacePrincipalWebApi());
            filters.Add(new SupervisorFunctionsEnabledAttribute());
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
            if (lastError.IsHttpNotFound()) return;

            this.logger.Error("Unexpected error occurred", lastError);
            if (lastError.InnerException != null)
            {
                this.logger.Error("Unexpected error occurred", lastError.InnerException);
            }
        }

        protected void Application_Start()
        {
            this.logger.Info("Starting application.");
            MvcHandler.DisableMvcResponseHeader = true;

            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;
            
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());
            //WebApiConfig.Register(GlobalConfiguration.Configuration);

            RegisterVirtualPathProvider();
            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // RouteTable.Routes.Add(new ServiceRoute("", new Ninject.Extensions.Wcf.NinjectServiceHostFactory(), typeof(API)));

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);
            RegisterRoutes(RouteTable.Routes);

            DataAnnotationsConfig.RegisterAdapters();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            try
            {
                var checkStatus = healthCheckService.Check();
                if (checkStatus.Status == HealthCheckStatus.Down)
                {
                    var hostName = Dns.GetHostName();
                    this.logger.Fatal(string.Format("Initial Health Check for {0} failed. Result: {1}", hostName , checkStatus.GetStatusDescription()));
                }
            }
            catch (Exception exc)
            {
                this.logger.Fatal("Error on checking application health.", exc);
            }
        }

        protected void Application_End()
        {
            this.logger.Info("Ending application.");
            this.logger.Info("ShutdownReason: " + HostingEnvironment.ShutdownReason.ToString());

            if (HostingEnvironment.ShutdownReason == ApplicationShutdownReason.HostingEnvironment)
            {
                var httpRuntimeType = typeof (HttpRuntime);
                var httpRuntime = httpRuntimeType.InvokeMember(
                    "_theRuntime",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                    null, null, null) as HttpRuntime;

                var shutDownMessage = httpRuntimeType.InvokeMember(
                    "_shutDownMessage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                    null, httpRuntime, null) as string;

                string shutDownStack = httpRuntimeType.InvokeMember("_shutDownStack",
                    BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.GetField,
                    null,
                    httpRuntime,
                    null) as string;

                this.logger.Info("ShutDownMessage: " + shutDownMessage);
                this.logger.Info("ShutDownStack: " + shutDownStack);
            }
        }

        private static void RegisterVirtualPathProvider()
        {
            Assembly[] assemblies = BuildManager
                .GetReferencedAssemblies()
                .Cast<Assembly>()
                .Where(assembly => assembly.GetName().Name.Contains("SharedKernels") && assembly.GetName().Name.Contains("Web"))
                .ToArray();

            HostingEnvironment.RegisterVirtualPathProvider(new Vpp(assemblies)
            {
                //you can do a specific assembly registration too. If you provide the assemly source path, it can read
                //from the source file so you can change the content while the app is running without needing to rebuild
                //{typeof(SomeAssembly.SomeClass).Assembly, @"..\SomeAssembly"} 
            });

            BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;
        }

        private void CurrentUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exp = (Exception)e.ExceptionObject;
                this.logger.Error("Global Unhandled:", exp);
            }
            catch (Exception)
            {
                
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

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;
            if (application != null && application.Context != null)
            {
                application.Context.Response.Headers.Remove("Server");
            }
        }

        
    }
}