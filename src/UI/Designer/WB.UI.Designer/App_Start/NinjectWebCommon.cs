using System;
using System.Web;
using Main.Core;
using Main.Core.Commands;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Indexes;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Snapshots;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Code;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Shared.Web.Modules;
using WebActivatorEx;
using CommandService = WB.Core.Infrastructure.CommandBus.CommandService;

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
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization

            var ravenSettings = new RavenConnectionSettings(storagePath: AppSettings.Instance.RavenDocumentStore,
                username: AppSettings.Instance.RavenUserName, password: AppSettings.Instance.RavenUserPassword,
                eventsDatabase: AppSettings.Instance.RavenEventsDatabase,
                viewsDatabase: AppSettings.Instance.RavenViewsDatabase,
                plainDatabase: AppSettings.Instance.RavenPlainDatabase,
                failoverBehavior: AppSettings.Instance.FailoverBehavior,
                activeBundles: AppSettings.Instance.ActiveBundles);

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new WebConfigurationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new RavenReadSideInfrastructureModule(ravenSettings, AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), typeof(DesignerReportQuestionnaireListViewItem).Assembly),
                new DesignerCommandDeserializationModule(),
                new ExpressionProcessorModule(),
                new DesignerBoundedContextModule(),
                new QuestionnaireVerificationModule(),
                new MembershipModule(),
                new MainModule(),
                new FileInfrastructureModule(),
                new DesignerRegistry()
                );
            kernel.Load(ModulesFactory.GetEventStoreModule());
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            PrepareNcqrsInfrastucture(kernel);
            
            return kernel;
        }

        private static void PrepareNcqrsInfrastucture(StandardKernel kernel)
        {
            var ncqrsCommandService = new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.SetDefault(ncqrsCommandService);
            NcqrsInit.InitializeCommandService(kernel.Get<ICommandListSupplier>(), ncqrsCommandService);

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());

            kernel.Bind<ISnapshottingPolicy>().ToMethod(context => NcqrsEnvironment.Get<ISnapshottingPolicy>());
            kernel.Bind<ISnapshotStore>().ToMethod(context => NcqrsEnvironment.Get<ISnapshotStore>());
            kernel.Bind<IAggregateRootCreationStrategy>().ToMethod(context => NcqrsEnvironment.Get<IAggregateRootCreationStrategy>());
            kernel.Bind<IAggregateSnapshotter>().ToMethod(context => NcqrsEnvironment.Get<IAggregateSnapshotter>());

            kernel.Bind<IDomainRepository>().To<DomainRepository>();

            CreateAndRegisterEventBus(kernel);

            kernel.Bind<IAggregateRootRepository>().To<AggregateRootRepository>();
            kernel.Bind<IEventPublisher>().To<EventPublisher>();
            kernel.Bind<ISnapshotManager>().To<SnapshotManager>();

            // TODO: TLK, KP-4337: make correct mapping here, not a direct creation
            var commandService = new CommandService(ncqrsCommandService, kernel.Get<IAggregateRootRepository>(), kernel.Get<IEventPublisher>(), kernel.Get<ISnapshotManager>());

            kernel.Bind<ICommandService>().ToConstant(commandService);
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
            var bus = new NcqrCompatibleEventDispatcher();

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }

            return bus;
        }
    }
}