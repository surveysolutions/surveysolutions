using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Http.Filters;
using System.Web.SessionState;
using Elmah;
using Microsoft.Practices.ServiceLocation;
using NConfig;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Elmah;
using WB.UI.Shared.Web.Filters;
using WB.UI.Supervisor.Filters;

namespace WB.UI.Supervisor
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Supervisor.App_Start;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Initialization per AppDomain.
        /// </summary>
        static MvcApplication()
        {
            SetupNConfig();
        }

        private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ReplacePrincipal());
            filters.Add(new RequireSecureConnectionAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LongWebsiteDirectoryPathFilter());
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ReplacePrincipalWebApi());
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
            MvcHandler.DisableMvcResponseHeader = true;

            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += this.CurrentUnhandledException;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);

            RegisterVirtualPathProvider();
            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // RouteTable.Routes.Add(new ServiceRoute("", new Ninject.Extensions.Wcf.NinjectServiceHostFactory(), typeof(API)));

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
            RegisterRoutes(RouteTable.Routes);

            DataAnnotationsConfig.RegisterAdapters();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            //AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
        }

        private static void RegisterVirtualPathProvider()
        {
            Assembly[] assemblies = BuildManager
                .GetReferencedAssemblies()
                .Cast<Assembly>()
                .Where(assembly => assembly.GetName().Name.Contains("SharedKernels") && assembly.GetName().Name.Contains("Web"))
                .ToArray();

            HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceVirtualPathProvider.Vpp(assemblies)
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