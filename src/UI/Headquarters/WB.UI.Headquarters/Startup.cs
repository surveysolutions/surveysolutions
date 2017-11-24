﻿using System;
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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using NConfig;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.Common.WebHost;
using Ninject.Web.WebApi.OwinHost;
using NLog;
using Npgsql;
using Owin;
using Prometheus.Advanced;
using Quartz;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters
{
    public class Startup
    {
        internal static void SetupNConfig()
        {
            NConfigurator.RegisterSectionMerger(new DeepMerger<HqSecuritySection>());
            NConfigurator.UsingFiles(@"~\Configuration\Headquarters.Web.config").SetAsSystemDefault();
        }

        static Startup()
        {
            SetupNConfig();
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
        }

        public void Configuration(IAppBuilder app)
        {
            DefaultCollectorRegistry.Instance.Clear();

            app.Use(RemoveServerNameFromHeaders);

            var kernel = ConfigureNinject(app);
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<Startup>();
            logger.Info($@"Starting Headquarters {ServiceLocator.Current.GetInstance<IProductVersion>()}");
            UpdateAppVersion();
            ConfigureAuth(app);
            InitializeAppShutdown(app);
            InitializeMVC();
            ConfigureWebApi(app);

            Settings.Current.GetCustomData += (exception, dictionary) =>
            {
                void AddAllSqlData(Exception e)
                {
                    if (e is Npgsql.PostgresException pe)
                    {
                        if (pe.InternalQuery != null)
                            exception.AddLogData(@"Internal Query", pe.InternalQuery);

                        if (pe.Statement?.SQL != null)
                            exception.AddLogData(@"SQL Statement", pe.Statement.SQL);
                    }

                    if (e.InnerException != null)
                    {
                        AddAllSqlData(e.InnerException);
                    }
                }

                //AddAllSqlData(exception);
            };

            Settings.Current.ExceptionActions.AddHandler<TargetInvocationException>((error, exception) =>
            {
                void AddAllSqlData(Exception e)
                {
                    if (e is Npgsql.PostgresException pe)
                    {
                        error.AddCommand(new Command("NpgSql", pe.Statement.SQL));
                    }

                    if (e.InnerException != null)
                    {
                        AddAllSqlData(e.InnerException);
                    }
                }

                AddAllSqlData(exception);
            });

            InitMetrics();
            
            StartMetricsPush(kernel, logger);
        }

        private static void InitMetrics()
        {
            CommonMetrics.StateFullInterviewsCount.Set(0);
        }

        private void StartMetricsPush(IKernel kernel, Core.GenericSubdomains.Portable.Services.ILogger logger)
        {
            try
            {
                // getting instance name from connection string information
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[@"Postgres"];
                var npgsConnectionString = new NpgsqlConnectionStringBuilder(connectionString.ConnectionString);
                var instanceName = npgsConnectionString.ApplicationName;

                // configuring address for metrics pushgateway
                var metricsGateway = System.Configuration.ConfigurationManager.AppSettings["Metrics.Gateway"];

                // initialize push mechanizm
                new Prometheus.MetricPusher(metricsGateway, 
                    job: "hq", 
                    additionalLabels: new[] { Tuple.Create("site", instanceName) },
                    intervalMilliseconds: 1000).Start();
            }
        catch (Exception e)
            {
                logger.Error("Unable to start metrics push", e);
            }
        }

        private IKernel ConfigureNinject(IAppBuilder app)
        {
            var perRequestModule = new OnePerRequestHttpModule();

            // onPerRequest scope implementation. Collecting all perRequest instances after all requests
            app.Use(async (ctx, next) =>
            {
                try
                {
                    if (ctx.Request.CallCancelled.IsCancellationRequested) return;

                    await next();
                }
                finally
                {
                    perRequestModule.DeactivateInstancesForCurrentHttpRequest();
                }
            });

            var kernel = NinjectConfig.CreateKernel();
            kernel.Inject(perRequestModule); // wiill keep reference to perRequestModule in Kernel instance
            app.UseNinjectMiddleware(() => kernel);
            return kernel;
        }

        private void ConfigureWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            WebApiConfig.Register(config);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            app.MapSignalR(new HubConfiguration { EnableDetailedErrors = true });
            app.Use(SetSessionStateBehavior).UseStageMarker(PipelineStage.MapHandler);

            app.UseNinjectWebApi(config);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var applicationSecuritySection = NConfigurator.Default.GetSection<HqSecuritySection>(@"applicationSecurity");

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(@"/Account/LogOn"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<HqUserManager, HqUser, Guid>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentityCallback: (manager, user) => manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie),
                            getUserIdCallback: (id) => Guid.Parse(id.GetUserId())),

                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request) && !IsApiRequest(ctx.Request) && !IsBasicAuthApiUnAuthRequest(ctx.Response))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                },
                ExpireTimeSpan = TimeSpan.FromHours(applicationSecuritySection.CookieSettings.ExpirationTime),
                SlidingExpiration = applicationSecuritySection.CookieSettings.SlidingExpiration,
                CookieName = applicationSecuritySection.CookieSettings.Name,
                CookieHttpOnly = applicationSecuritySection.CookieSettings.HttpOnly
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }

        private static bool IsApiRequest(IOwinRequest request)
        {
            var userAgent = request.Headers[@"User-Agent"];
            return (userAgent?.ToLowerInvariant().Contains(@"org.worldbank.solutions.") ?? false) || (userAgent?.Contains(@"okhttp/") ?? false);
        }

        private static bool IsBasicAuthApiUnAuthRequest(IOwinResponse response)
        {
            return response.Headers[ApiBasicAuthAttribute.AuthHeader] != null;
        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }

        private static Task SetSessionStateBehavior(IOwinContext context, Func<Task> next)
        {
            // Depending on the handler the request gets mapped to, session might not be enabled. Force it on.
            HttpContextBase httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
            return next();
        }

        private static Task RemoveServerNameFromHeaders(IOwinContext context, Func<Task> next)
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
            InitMetrics();

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info(@"Ending application.");
            logger.Info(@"ShutdownReason: " + HostingEnvironment.ShutdownReason);

            ServiceLocator.Current.GetInstance<IScheduler>()?.Shutdown();

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

        private static void UpdateAppVersion()
            => ServiceLocator.Current.GetInstance<IProductVersionHistory>().RegisterCurrentVersion();

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new NlogExceptionFilter());
            filters.Add(new RequireSecureConnectionAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new InstallationAttribute(), 100);
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ErrorLoggerFilter());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
        }
    }
}