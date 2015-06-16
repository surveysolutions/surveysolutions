using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Designer;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Storage.Esent;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Modules;
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

            var ravenSettings = new RavenConnectionSettings(
                storagePath: AppSettings.Instance.RavenDocumentStore,
                username: AppSettings.Instance.RavenUserName,
                password: AppSettings.Instance.RavenUserPassword,
                viewsDatabase: AppSettings.Instance.RavenViewsDatabase,
                plainDatabase: AppSettings.Instance.RavenPlainDatabase,
                failoverBehavior: AppSettings.Instance.FailoverBehavior,
                activeBundles: AppSettings.Instance.ActiveBundles,
                ravenFileSystemName: AppSettings.Instance.RavenFileSystemName);

            var dynamicCompilerSettings = (DynamicCompilerSettings)WebConfigurationManager.GetSection("dynamicCompilerSettings");

            string appDataDirectory = WebConfigurationManager.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = System.Web.Hosting.HostingEnvironment.MapPath(appDataDirectory);
            }

            string esentDataFolder = Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings["Esent.DbFolder"]);
            string plainEsentDataFolder = Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings["Esent.Plain.DbFolder"]);

            int esentCacheSize = WebConfigurationManager.AppSettings["Esent.CacheSize"].ParseIntOrNull() ?? 256;

            int postgresCacheSize = WebConfigurationManager.AppSettings["Postgres.CacheSize"].ParseIntOrNull() ?? 1024;
            var mappingAssemblies = new List<Assembly> { typeof(DesignerBoundedContextModule).Assembly }; 

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new PostgresReadSideModule(WebConfigurationManager.ConnectionStrings["ReadSide"].ConnectionString, postgresCacheSize, mappingAssemblies),
                new DesignerRegistry(),
                new DesignerCommandDeserializationModule(),
                new EsentReadSideModule(esentDataFolder, plainEsentDataFolder, esentCacheSize),
                new DesignerBoundedContextModule(dynamicCompilerSettings),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new MainModule(),
                new FileInfrastructureModule()
                );
            NcqrsEnvironment.SetGetter<ILogger>(() => kernel.Get<ILogger>());
            NcqrsEnvironment.InitDefaults();
            kernel.Load(ModulesFactory.GetEventStoreModule());
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            kernel.Bind<ReadSideService>().ToSelf().InSingletonScope();
            kernel.Bind<IReadSideStatusService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
            kernel.Bind<IReadSideAdministrationService>().ToMethod(context => context.Kernel.Get<ReadSideService>());

            PrepareNcqrsInfrastucture(kernel);
            
            return kernel;
        }

        private static void PrepareNcqrsInfrastucture(StandardKernel kernel)
        {
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());

            kernel.Bind<ISnapshottingPolicy>().ToMethod(context => NcqrsEnvironment.Get<ISnapshottingPolicy>());
            kernel.Bind<ISnapshotStore>().ToMethod(context => NcqrsEnvironment.Get<ISnapshotStore>());
            kernel.Bind<IAggregateRootCreationStrategy>().ToMethod(context => NcqrsEnvironment.Get<IAggregateRootCreationStrategy>());
            kernel.Bind<IAggregateSnapshotter>().ToMethod(context => NcqrsEnvironment.Get<IAggregateSnapshotter>());

            CreateAndRegisterEventBus(kernel);
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            NcqrsEnvironment.SetGetter<IEventBus>(() => GetEventBus(kernel));
            kernel.Bind<IEventBus>().ToMethod(_ => GetEventBus(kernel));
            kernel.Bind<IEventDispatcher>().ToMethod(_ => GetEventBus(kernel));
        }

        private static NcqrCompatibleEventDispatcher GetEventBus(StandardKernel kernel)
        {
            return eventDispatcher ?? (eventDispatcher = CreateEventBus(kernel));
        }

        private static NcqrCompatibleEventDispatcher CreateEventBus(StandardKernel kernel)
        {
            var ignoredDenormalizersConfigSection =
               (IgnoredDenormalizersConfigSection)WebConfigurationManager.GetSection("IgnoredDenormalizersSection");
            Type[] handlersToIgnore = ignoredDenormalizersConfigSection == null ? new Type[0] : ignoredDenormalizersConfigSection.GetIgnoredTypes();

            var bus = new NcqrCompatibleEventDispatcher(kernel.Get<IEventStore>(), handlersToIgnore);
            bus.TransactionManager = kernel.Get<ITransactionManagerProvider>();

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }

            return bus;
        }
    }
}