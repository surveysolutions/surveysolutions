using System;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using NConfig;
using NLog;
using NLog.Targets;
using Owin;
using Quartz;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Logging.Slack;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.DataAnnotations;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

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
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(@"en-US");
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            //NpgsqlLogManager.Provider = new NLogNpgsqlLoggingProvider();
            //NpgsqlLogManager.IsParameterLoggingEnabled = true;
        }

        public void Configuration(IAppBuilder app)
        {
            Target.Register<SlackFatalNotificationsTarget>("slack");
            
            ConfigureExceptionalStore();
            app.Use(RemoveServerNameFromHeaders);

            var autofacKernel = AutofacConfig.CreateKernel();

            autofacKernel.ContainerBuilder.RegisterHubs(Assembly.GetAssembly(typeof(WebInterviewHub)));
            autofacKernel.ContainerBuilder.RegisterControllers(typeof(Startup).Assembly);
            autofacKernel.ContainerBuilder.RegisterApiControllers(typeof(Startup).Assembly);
            
            autofacKernel.ContainerBuilder.RegisterType<Autofac.Integration.SignalR.AutofacDependencyResolver>()
                .As<Microsoft.AspNet.SignalR.IDependencyResolver>().SingleInstance();
            
            autofacKernel.ContainerBuilder.RegisterFilterProvider();

            var config = new HttpConfiguration();
            autofacKernel.ContainerBuilder.RegisterWebApiFilterProvider(config);
            autofacKernel.ContainerBuilder.RegisterWebApiModelBinderProvider();

            var initTask = autofacKernel.InitAsync(
                    System.Configuration.ConfigurationManager.AppSettings
                        .GetBool("RestartAppPoolOnInitializationError", true)
                );

            if (CoreSettings.IsDevelopmentEnvironment)
                initTask.Wait();
            else
                initTask.Wait(TimeSpan.FromSeconds(10));

            var container = autofacKernel.Container;

            InScopeExecutor.Init(new UnitOfWorkInScopeExecutor(container));

            var resolver = new AutofacWebApiDependencyResolver(container);
            config.DependencyResolver = resolver;
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

            var hubActivator = new CustomAutofacHubActivator(container);
            var signalRAutofacDependencyResolver = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>();
            
            signalRAutofacDependencyResolver.Register(typeof(IHubActivator), () => hubActivator);
            
            GlobalHost.DependencyResolver = signalRAutofacDependencyResolver;
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            ModelBinders.Binders.DefaultBinder = new AutofacBinderResolver(container);

            var scopeResolver = new AutofacServiceLocatorAdapter(container);
            ServiceLocator.SetLocatorProvider(() => scopeResolver);

            app.UseAutofacMiddleware(container);
            
            var logger = container.Resolve<ILoggerProvider>().GetFor<Startup>();
            logger.Info($@"Starting Headquarters {container.Resolve<IProductVersion>()}");

            ConfigureAuth(app, container);
            
            InitializeMVC();
            ConfigureWebApi(app, config);

            app.UseWebApi(config);

            var scheduler = container.Resolve<IScheduler>();
            scheduler.Start();

            InitializeAppShutdown(app, scheduler);

            AntiForgeryConfig.RequireSsl = CoreSettings.IsHttpsRequired;

            Exceptional.Settings.ExceptionActions.AddHandler<TargetInvocationException>((error, exception) =>
            {
                void AddAllSqlData(Exception e)
                {
                    if (e is Npgsql.PostgresException pe)
                    {
                        error.AddCommand(new Command(@"NpgSql", pe.Statement.SQL));
                    }

                    if (e.InnerException != null)
                    {
                        AddAllSqlData(e.InnerException);
                    }
                }

                AddAllSqlData(exception);
            });
        }


        private void ConfigureWebApi(IAppBuilder app, HttpConfiguration config)
        {
            config.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter());

            GlobalConfiguration.Configure(WebApiConfig.Register);
            WebApiConfig.Register(config);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            WebInterviewModuleConfigure(app);
            app.Use(SetSessionStateBehavior).UseStageMarker(PipelineStage.MapHandler);

            app.UseAutofacWebApi(config);
        }

        public static void WebInterviewModuleConfigure(IAppBuilder app)
        {
            var resolver = GlobalHost.DependencyResolver;
            (resolver.GetService(typeof(IConnectionsMonitor)) as IConnectionsMonitor)?.StartMonitoring();
            app.MapSignalR(new HubConfiguration { EnableDetailedErrors = true, Resolver = resolver });
        }

        private void ConfigureAuth(IAppBuilder app, ILifetimeScope ioc)
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
                    OnValidateIdentity = OnValidateIdentity(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentityCallback: (manager, user) => manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie),
                            getUserIdCallback: (id) => Guid.Parse(id.GetUserId()),
                            ioc: ioc),

                    OnApplyRedirect = ctx => ctx.ApplyNonApiRedirect()
                },
                ExpireTimeSpan = TimeSpan.FromHours(applicationSecuritySection.CookieSettings.ExpirationTime),
                SlidingExpiration = applicationSecuritySection.CookieSettings.SlidingExpiration,
                CookieName = applicationSecuritySection.CookieSettings.Name,
                CookieHttpOnly = applicationSecuritySection.CookieSettings.HttpOnly
            });
        }

        /// <summary>
        ///     Can be used as the ValidateIdentity method for a CookieAuthenticationProvider which will check a user's security
        ///     stamp after validateInterval
        ///     Rejects the identity if the stamp changes, and otherwise will call regenerateIdentity to sign in a new
        ///     ClaimsIdentity
        /// </summary>
        /// <param name="validateInterval"></param>
        /// <param name="regenerateIdentityCallback"></param>
        /// <param name="getUserIdCallback"></param>
        /// <param name="ioc"></param>
        /// <returns></returns>
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity(
            TimeSpan validateInterval, 
            Func<HqUserManager, HqUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
            Func<ClaimsIdentity, Guid> getUserIdCallback,
            ILifetimeScope ioc)
        {
            if (getUserIdCallback == null)
                throw new ArgumentNullException(nameof(getUserIdCallback));

            return async context =>
            {
                var currentUtc = DateTimeOffset.UtcNow;

                if (context.Options?.SystemClock != null)
                    currentUtc = context.Options.SystemClock.UtcNow;

                var issuedUtc = context.Properties.IssuedUtc;

                // Only validate if enough time has elapsed
                var validate = (issuedUtc == null);
                if (issuedUtc != null)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    validate = timeElapsed > validateInterval;
                }

                if (validate)
                {
                    var manager = ioc.Resolve<HqUserManager>();
                    var userId = getUserIdCallback(context.Identity);
                    if (manager != null && userId != null)
                    {
                        var user = await manager.FindByIdAsync(userId).WithCurrentCulture();
                        var reject = true;
                        // Refresh the identity if the stamp matches, otherwise reject
                        if (user != null && manager.SupportsUserSecurityStamp)
                        {
                            var securityStamp =
                                context.Identity.FindFirstValue(Constants.DefaultSecurityStampClaimType);
                            if (securityStamp == await manager.GetSecurityStampAsync(userId).WithCurrentCulture())
                            {
                                reject = false;
                                // Regenerate fresh claims if possible and resign in
                                if (regenerateIdentityCallback != null)
                                {
                                    var identity = await regenerateIdentityCallback.Invoke(manager, user).WithCurrentCulture();
                                    if (identity != null)
                                    {
                                        // Fix for regression where this value is not updated
                                        // Setting it to null so that it is refreshed by the cookie middleware
                                        context.Properties.IssuedUtc = null;
                                        context.Properties.ExpiresUtc = null;
                                        context.OwinContext.Authentication.SignIn(context.Properties, identity);
                                    }
                                }
                            }
                        }
                        if (reject)
                        {
                            context.RejectIdentity();
                            context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                        }
                    }
                }
            };
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

        private static void InitializeAppShutdown(IAppBuilder app, IScheduler scheduler)
        {
            var properties = new AppProperties(app.Properties);
            CancellationToken token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
                token.Register(()=> OnShutdown(scheduler));
        }

        private static void OnShutdown(IScheduler scheduler)
        {
            CommonMetrics.StateFullInterviewsCount.Set(0);

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info(@"Ending application.");
            logger.Info(@"ShutdownReason: " + HostingEnvironment.ShutdownReason);

            scheduler.Shutdown();

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
            RazorGeneratorMvcStart.Start();

            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
        }

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

        private void ConfigureExceptionalStore()
        {
            if (Exceptional.Settings.DefaultStore is PostgreSqlErrorStore postgresStore)
            {
                postgresStore.Settings.TableName = @"""logs"".""Errors""";
            }

            if (Exceptional.Settings.DefaultStore is JSONErrorStore exceptionalConfig)
            {
                var jsonStorePath = exceptionalConfig.Settings.Path;
                var jsonStorePathAbsolute = HostingEnvironment.MapPath(jsonStorePath);

                if (!Directory.Exists(jsonStorePathAbsolute))
                {
                    Directory.CreateDirectory(jsonStorePathAbsolute);
                }
            }
        }
    }
}
