using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
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
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Supervisor.Code;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.Injections;
using WB.UI.Supervisor.App_Start;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace WB.UI.Supervisor.App_Start
{
    using Microsoft.Practices.ServiceLocation;

    using NinjectAdapter;


    /// <summary>
    /// The ninject web common.
    /// </summary>
    public static class NinjectWebCommon
    {
        /// <summary>
        /// The bootstrapper.
        /// </summary>
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
#warning TLK: delete this when NCQRS initialization moved to Global.asax
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization


            string storePath = WebConfigurationManager.AppSettings["Raven.DocumentStore"];

            var ravenSettings = new RavenConnectionSettings(storePath,
                username: WebConfigurationManager.AppSettings["Raven.Username"],
                password: WebConfigurationManager.AppSettings["Raven.Password"],
                eventsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"],
                activeBundles: WebConfigurationManager.AppSettings["Raven.Databases.ActiveBundles"]);

            
            var schedulerSettings = new SchedulerSettings(LegacyOptions.SchedulerEnabled,
                int.Parse(WebConfigurationManager.AppSettings["Scheduler.HqSynchronizationInterval"]));

            var headquartersSettings = (HeadquartersSettings) System.Configuration.ConfigurationManager.GetSection(
                "headquartersSettingsGroup/headquartersSettings");

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                    LegacyOptions.InterviewDetailsDataSchedulerNumberOfInterviewsProcessedAtTime);

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof (AccountController).Assembly.GetName().Version;

            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: true,
                appDataDirectory: AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                incomingCapiPackagesDirectoryName: LegacyOptions.SynchronizationIncomingCapiPackagesDirectory,
                incomingCapiPackagesWithErrorsDirectoryName:
                    LegacyOptions.SynchronizationIncomingCapiPackagesWithErrorsDirectory,
                incomingCapiPackageFileNameExtension: LegacyOptions.SynchronizationIncomingCapiPackageFileNameExtension);

            var basePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            const string QuestionnaireAssembliesFolder = "QuestionnaireAssemblies";

            var ravenReadSideRepositoryWriterSettings = new RavenReadSideRepositoryWriterSettings(basePath, int.Parse(WebConfigurationManager.AppSettings["Raven.Readside.BulkInsertBatchSize"]));

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: true, basePath: basePath),
                new QuestionnaireUpgraderModule(),
                new RavenReadSideInfrastructureModule(ravenSettings, ravenReadSideRepositoryWriterSettings, typeof(SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly),
                new RavenPlainStorageInfrastructureModule(ravenSettings),
                new FileInfrastructureModule(),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new SupervisorBoundedContextModule(headquartersSettings, schedulerSettings));

            NcqrsEnvironment.SetGetter<ILogger>(() => kernel.Get<ILogger>());
            NcqrsEnvironment.InitDefaults();

            kernel.Bind<ISettingsProvider>().To<SupervisorSettingsProvider>();

            var eventStoreModule = ModulesFactory.GetEventStoreModule();
            var overrideReceivedEventTimeStamp = CoreSettings.EventStoreProvider == StoreProviders.Raven;

            kernel.Load(
                eventStoreModule,
                new SurveyManagementSharedKernelModule(basePath,
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Major"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Minor"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Patch"]), isDebug,
                    applicationBuildVersion, interviewDetailsDataLoaderSettings, overrideReceivedEventTimeStamp,
                    Constants.CapiSynchronizationOrigin, false,
                    int.Parse(WebConfigurationManager.AppSettings["Export.MaxCountOfCachedEntitiesForSqliteDb"])));


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            PrepareNcqrsInfrastucture(kernel);

            ServiceLocator.Current.GetInstance<BackgroundSyncronizationTasks>().Configure();


            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<IScheduler>().Start();

            kernel.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();

            kernel.Bind<ITokenVerifier>().To<ApiValidationAntiForgeryTokenVerifier>().InSingletonScope();
            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>();

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
            var ignoredDenormalizersConfigSection =
               (IgnoredDenormalizersConfigSection)WebConfigurationManager.GetSection("IgnoredDenormalizersSection");
            Type[] handlersToIgnore = ignoredDenormalizersConfigSection == null ? new Type[0] : ignoredDenormalizersConfigSection.GetIgnoredTypes();
            var bus = new NcqrCompatibleEventDispatcher(kernel.Get<IEventStore>(), handlersToIgnore);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }
        }
    }
}