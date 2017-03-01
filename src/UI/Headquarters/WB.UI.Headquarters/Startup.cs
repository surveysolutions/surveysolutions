using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Practices.ServiceLocation;
using NConfig;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Prometheus;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Identity;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters
{
    public class Startup
    {
        private static readonly Counter ExceptionsLogged = Prometheus.Metrics.CreateCounter(@"exceptions_raised", @"Total exceptions raised");

        private static void SetupNConfig()
            => NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();

        static Startup()
        {
            SetupNConfig();
        }

        public void Configuration(IAppBuilder app)
        {
            var ninjectKernel = NinjectConfig.CreateKernel();

            ConfigureAuth(app);

            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();

            logger.Info($"Starting Headquarters {ServiceLocator.Current.GetInstance<IProductVersion>()}");

            InitializeAppShutdown(app);
            UpdateAppVersion();
            HealthCheck();

            InitializeMVC();

            var config = new HttpConfiguration();
            config.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            WebApiConfig.Register(config);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            app.Use(RemoverServerName)
                .Use(SetSessionStateBehavior)
                .UseStageMarker(PipelineStage.MapHandler)
                .UseNinjectMiddleware(() => ninjectKernel)
                .UseNinjectWebApi(config).MapSignalR(new HubConfiguration
            {
                EnableDetailedErrors = true
            });
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(@"/Account/LogOn"),
                Provider = new CookieAuthenticationProvider { }
        });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }

        private static Task SetSessionStateBehavior(IOwinContext context, Func<Task> next)
        {
            // Depending on the handler the request gets mapped to, session might not be enabled. Force it on.
            HttpContextBase httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
            return next();
        }

        private static Task RemoverServerName(IOwinContext context, Func<Task> next)
        {
            context.Response.Headers.Remove(@"Server");
            return next.Invoke();
        }

        private static void InitializeAppShutdown(IAppBuilder app)
        {
            var properties = new AppProperties(app.Properties);
            CancellationToken token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
                token.Register(OnShutdown);
        }

        private static void OnShutdown()
        {
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();

            logger.Info(@"Ending application.");
            logger.Info(@"ShutdownReason: " + HostingEnvironment.ShutdownReason);

            if (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.HostingEnvironment) return;

            var httpRuntimeType = typeof(HttpRuntime);
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

            logger.Info(@"ShutDownMessage: " + shutDownMessage);
            logger.Info(@"ShutDownStack: " + shutDownStack);
        }

        private static void InitializeMVC()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);

            DataAnnotationsConfig.RegisterAdapters();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();

            try
            {
                var exception = (Exception) e.ExceptionObject;
                logger.Error(@"Global Unhandled:", exception);

                ExceptionsLogged.Inc();
            }
            catch
            {
                // ignored
            }
        }

        private static void UpdateAppVersion()
            => ServiceLocator.Current.GetInstance<IProductVersionHistory>().RegisterCurrentVersion();

        private static void HealthCheck()
        {
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();
            var healthCheckService = ServiceLocator.Current.GetInstance<IHealthCheckService>();

            try
            {
                var checkStatus = healthCheckService.Check();
                if (checkStatus.Status == HealthCheckStatus.Down)
                {
                    logger.Fatal($"Initial Health Check for {Dns.GetHostName()} failed. " +
                                 $"Result: {checkStatus.GetStatusDescription()}");
                }
            }
            catch (Exception exc)
            {
                logger.Fatal(@"Error on checking application health.", exc);
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequireSecureConnectionAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MaintenanceFilter());
            filters.Add(new InstallationAttribute(), 100);
            
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new ApiMaintenanceFilter());
        }
    }
}
