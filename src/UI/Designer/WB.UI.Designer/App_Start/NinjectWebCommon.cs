using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http.Filters;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Native.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;

using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace WB.UI.Designer.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        private static NcqrCompatibleEventDispatcher eventDispatcher;

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            // HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            var dynamicCompilerSettings = (IDynamicCompilerSettingsGroup)WebConfigurationManager.GetSection("dynamicCompilerSettingsGroup");
            string appDataDirectory = HostingEnvironment.MapPath("~/App_Data");

            int postgresCacheSize = WebConfigurationManager.AppSettings["Postgres.CacheSize"].ParseIntOrNull() ?? 1024;
            string esentCacheFolder = Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings["Esent.Cache.Folder"] ?? @"Temp\EsentCache");
            var cacheSettings = new ReadSideCacheSettings(esentCacheFolder, postgresCacheSize, postgresCacheSize / 2);

            var mappingAssemblies = new List<Assembly> { typeof(DesignerBoundedContextModule).Assembly };

            var readSideSettings = new ReadSideSettings(
                WebConfigurationManager.AppSettings["ReadSide.Version"].ParseIntOrNull() ?? 0);

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = WebConfigurationManager.ConnectionStrings["PlainStore"].ConnectionString,
                MappingAssemblies = new List<Assembly> { typeof(DesignerBoundedContextModule).Assembly}
            };

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new PostgresKeyValueModule(cacheSettings),
                new PostgresPlainStorageModule(postgresPlainStorageSettings),
                new PostgresReadSideModule(WebConfigurationManager.ConnectionStrings["ReadSide"].ConnectionString, cacheSettings, mappingAssemblies),
                new DesignerRegistry(),
                new DesignerCommandDeserializationModule(),
                new DesignerBoundedContextModule(dynamicCompilerSettings),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new MainModule(),
                new FileInfrastructureModule()
                );

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>()
                .WithConstructorArgument("tokenVerifier", new ApiValidationAntiForgeryTokenVerifier());

            kernel.Bind<ISettingsProvider>().To<DesignerSettingsProvider>().InSingletonScope();
            kernel.Load(ModulesFactory.GetEventStoreModule());
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            kernel.Bind<ReadSideSettings>().ToConstant(readSideSettings);
            kernel.Bind<ReadSideService>().ToSelf().InSingletonScope();
            kernel.Bind<IReadSideStatusService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
            kernel.Bind<IReadSideAdministrationService>().ToMethod(context => context.Kernel.Get<ReadSideService>());

            kernel.Bind<IAuthenticationService>().To<AuthenticationService>();
            kernel.Bind<IRecaptchaService>().To<RecaptchaService>();

            CreateAndRegisterEventBus(kernel);
            
            return kernel;
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            kernel.Bind<IEventBus>().ToMethod(_ => GetEventBus(kernel));
            kernel.Bind<ILiteEventBus>().ToMethod(_ => GetEventBus(kernel));
            kernel.Bind<IEventDispatcher>().ToMethod(_ => GetEventBus(kernel));
        }

        private static NcqrCompatibleEventDispatcher GetEventBus(StandardKernel kernel)
        {
            return eventDispatcher ?? (eventDispatcher = CreateEventBus(kernel));
        }

        private static NcqrCompatibleEventDispatcher CreateEventBus(StandardKernel kernel)
        {
            var eventBusConfigSection =
               (EventBusConfigSection)WebConfigurationManager.GetSection("eventBus");

            var bus = new NcqrCompatibleEventDispatcher(kernel.Get<IEventStore>(),
                 eventBusConfigSection.GetSettings(),
                kernel.Get<ILogger>());

            bus.TransactionManager = kernel.Get<ITransactionManagerProvider>();

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }

            return bus;
        }
    }
}