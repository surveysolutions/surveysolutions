using System;
using System.IO;
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
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.Storage.Esent;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
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
using WB.UI.Supervisor.Injections;
using WB.UI.Supervisor.App_Start;
using WB.UI.Supervisor.Controllers;
using WebActivatorEx;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace WB.UI.Supervisor.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
#warning TLK: delete this when NCQRS initialization moved to Global.asax
            MvcApplication.Initialize(); // pinging global.asax to perform it's part of static initialization


            string storePath = WebConfigurationManager.AppSettings["Raven.DocumentStore"];

            var ravenSettings = new RavenConnectionSettings(storePath,
                username: WebConfigurationManager.AppSettings["Raven.Username"],
                password: WebConfigurationManager.AppSettings["Raven.Password"],
                viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"],
                activeBundles: WebConfigurationManager.AppSettings["Raven.Databases.ActiveBundles"],
                ravenFileSystemName: WebConfigurationManager.AppSettings["Raven.Databases.RavenFileSystemName"]);

            
            var schedulerSettings = new SchedulerSettings(LegacyOptions.SchedulerEnabled,
                int.Parse(WebConfigurationManager.AppSettings["Scheduler.HqSynchronizationInterval"]));

            var headquartersSettings = (HeadquartersSettings) System.Configuration.ConfigurationManager.GetSection(
                "headquartersSettingsGroup/headquartersSettings");

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval);

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof(AccountController).Assembly.GetName().Version;

            string appDataDirectory = WebConfigurationManager.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = System.Web.Hosting.HostingEnvironment.MapPath(appDataDirectory);
            }

            var synchronizationSettings = new SyncSettings(appDataDirectory: appDataDirectory,
                incomingCapiPackagesWithErrorsDirectoryName:
                    LegacyOptions.SynchronizationIncomingCapiPackagesWithErrorsDirectory,
                incomingCapiPackageFileNameExtension: LegacyOptions.SynchronizationIncomingCapiPackageFileNameExtension,
                incomingUnprocessedPackagesDirectoryName: LegacyOptions.IncomingUnprocessedPackageFileNameExtension,
                origin: Constants.CapiSynchronizationOrigin, 
                retryCount: int.Parse(WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.RetryCount"]),
                retryIntervalInSeconds: LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval);

            var basePath = appDataDirectory;

            var ravenReadSideRepositoryWriterSettings = new RavenReadSideRepositoryWriterSettings(int.Parse(WebConfigurationManager.AppSettings["Raven.Readside.BulkInsertBatchSize"]));

            string esentDataFolder = Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings["Esent.DbFolder"]);

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: true, basePath: basePath),
                new QuestionnaireUpgraderModule(),
                new RavenReadSideInfrastructureModule(ravenSettings, ravenReadSideRepositoryWriterSettings, typeof(SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly, typeof(UserSyncPackagesByBriefFields).Assembly),
                new RavenPlainStorageInfrastructureModule(ravenSettings),
                new FileInfrastructureModule(),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new EsentReadSideModule(esentDataFolder),
                new SupervisorBoundedContextModule(headquartersSettings, schedulerSettings));

            NcqrsEnvironment.SetGetter<ILogger>(() => kernel.Get<ILogger>());
            NcqrsEnvironment.InitDefaults();

            kernel.Bind<ISettingsProvider>().To<SupervisorSettingsProvider>();

            var eventStoreModule = ModulesFactory.GetEventStoreModule();

            kernel.Load(
                eventStoreModule,
                new SurveyManagementSharedKernelModule(basePath, isDebug,
                    applicationBuildVersion, interviewDetailsDataLoaderSettings, false,
                    int.Parse(WebConfigurationManager.AppSettings["Export.MaxCountOfCachedEntitiesForSqliteDb"]),
                    new InterviewHistorySettings(basePath, false),
                    isSupervisorFunctionsEnabled: true));


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
            NcqrsEnvironment.SetDefault<ILiteEventBus>(bus);
            kernel.Bind<ILiteEventBus>().ToConstant(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }
        }
    }
}