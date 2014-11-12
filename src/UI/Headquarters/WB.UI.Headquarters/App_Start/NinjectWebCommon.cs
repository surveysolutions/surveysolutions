using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.WebApi.FilterBindingSyntax;
using Quartz;
using WB.Core.BoundedContexts.Headquarters;
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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Core.Synchronization;
using WB.UI.Headquarters;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Injections;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WebActivatorEx;
using CommandService = WB.Core.Infrastructure.CommandBus.CommandService;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace WB.UI.Headquarters
{
    /// <summary>
    ///     The ninject web common.
    /// </summary>
    public static class NinjectWebCommon
    {
        /// <summary>
        ///     The bootstrapper.
        /// </summary>
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            Global.Initialize(); // pinging global.asax to perform it's part of static initialization

            bool isEmbeded;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.IsEmbeded"], out isEmbeded))
            {
                isEmbeded = false;
            }

            string storePath = isEmbeded
                ? WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"]
                : WebConfigurationManager.AppSettings["Raven.DocumentStore"];

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof(SyncController).Assembly.GetName().Version;

            var ravenSettings = new RavenConnectionSettings(storePath, isEmbeded, WebConfigurationManager.AppSettings["Raven.Username"],
                WebConfigurationManager.AppSettings["Raven.Password"], WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"],
                activeBundles: WebConfigurationManager.AppSettings["Raven.Databases.ActiveBundles"]);

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                    LegacyOptions.InterviewDetailsDataSchedulerNumberOfInterviewsProcessedAtTime);

            bool reevaluateInterviewWhenSynchronized = LegacyOptions.SupervisorFunctionsEnabled;
            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: reevaluateInterviewWhenSynchronized,
                appDataDirectory: AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                incomingCapiPackagesDirectoryName: LegacyOptions.SynchronizationIncomingCapiPackagesDirectory,
                incomingCapiPackagesWithErrorsDirectoryName:
                    LegacyOptions.SynchronizationIncomingCapiPackagesWithErrorsDirectory,
                incomingCapiPackageFileNameExtension: LegacyOptions.SynchronizationIncomingCapiPackageFileNameExtension);

            var basePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            //const string QuestionnaireAssembliesFolder = "QuestionnaireAssemblies";

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new WebConfigurationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false, basePath: basePath),
                new QuestionnaireUpgraderModule(),
                new RavenReadSideInfrastructureModule(ravenSettings, basePath, typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly),
                new RavenPlainStorageInfrastructureModule(ravenSettings),
                new FileInfrastructureModule(),
                new HeadquartersRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new HeadquartersBoundedContextModule(LegacyOptions.SupervisorFunctionsEnabled));

            var eventStoreModule = ModulesFactory.GetEventStoreModule();
            var overrideReceivedEventTimeStamp = CoreSettings.EventStoreProvider == StoreProviders.Raven;

            kernel.Load(
                eventStoreModule,
                new SurveyManagementSharedKernelModule(basePath,
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Major"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Minor"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Patch"]), isDebug,
                    applicationBuildVersion, interviewDetailsDataLoaderSettings, overrideReceivedEventTimeStamp, Constants.SupervisorSynchronizationOrigin, true));


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            PrepareNcqrsInfrastucture(kernel);

            kernel.Bind<ITokenVerifier>().ToConstant(new SimpleTokenVerifier(WebConfigurationManager.AppSettings["Synchronization.Key"]));

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .WhenControllerHas<TokenValidationAuthorizationAttribute>();

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>()
                .WithConstructorArgument("tokenVerifier", new ApiValidationAntiForgeryTokenVerifier());

            kernel.BindHttpFilter<HeadquarterFeatureOnlyFilter>(System.Web.Http.Filters.FilterScope.Controller)
               .WhenControllerHas<HeadquarterFeatureOnlyAttribute>();

            kernel.Bind<IIdentityManager>().To<IdentityManager>().InSingletonScope();


            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<IScheduler>().Start();

            kernel.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();

            
#warning dirty index registrations
            // SuccessMarker.Start(kernel);
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
            var bus = new NcqrCompatibleEventDispatcher();
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);
            foreach (object handler in kernel.GetAll(typeof(IEventHandler)))
            {
                bus.Register((IEventHandler)handler);
            }
        }
    }
}