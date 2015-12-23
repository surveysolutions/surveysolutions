using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
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
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Native.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Supervisor.App_Start;
using WB.UI.Supervisor.Code;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.Injections;
using WebActivatorEx;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using FilterScope = System.Web.Http.Filters.FilterScope;

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
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();

            var schedulerSettings = new SchedulerSettings(LegacyOptions.SchedulerEnabled,
                int.Parse(WebConfigurationManager.AppSettings["Scheduler.HqSynchronizationInterval"]));

            var headquartersSettings = (HeadquartersSettings) ConfigurationManager.GetSection(
                "headquartersSettingsGroup/headquartersSettings");

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval);

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof(AccountController).Assembly.GetName().Version;

            string appDataDirectory = WebConfigurationManager.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = HostingEnvironment.MapPath(appDataDirectory);
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

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings
            {
                ConnectionString = WebConfigurationManager.ConnectionStrings["PlainStore"].ConnectionString,
                MappingAssemblies = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly, typeof(SupervisorBoundedContextModule).Assembly, typeof(SynchronizationModule).Assembly }
            };

            var readSideMaps = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly, typeof(SupervisorBoundedContextModule).Assembly }; 
            
            var cacheSettings = new ReadSideCacheSettings(
                enableEsentCache: WebConfigurationManager.AppSettings.GetBool("Esent.Cache.Enabled", @default: true),
                esentCacheFolder: Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings.GetString("Esent.Cache.Folder", @default: @"Temp\EsentCache")),
                cacheSizeInEntities: WebConfigurationManager.AppSettings.GetInt("ReadSide.CacheSize", @default: 1024),
                storeOperationBulkSize: WebConfigurationManager.AppSettings.GetInt("ReadSide.BulkSize", @default: 512));

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new SurveyManagementDataCollectionSharedKernelModule(usePlainQuestionnaireRepository: true, basePath: basePath),
                new QuestionnaireUpgraderModule(),
                new PostgresKeyValueModule(cacheSettings),
                new PostgresPlainStorageModule(postgresPlainStorageSettings),
                new PostgresReadSideModule(WebConfigurationManager.ConnectionStrings["ReadSide"].ConnectionString, cacheSettings, readSideMaps),
                new FileInfrastructureModule(),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new SupervisorBoundedContextModule(headquartersSettings, schedulerSettings));

            kernel.Bind<ISettingsProvider>().To<SupervisorSettingsProvider>();

            var eventStoreModule = ModulesFactory.GetEventStoreModule();

            var interviewCountLimitString = WebConfigurationManager.AppSettings["Limits.MaxNumberOfInterviews"];
            int? interviewCountLimit = string.IsNullOrEmpty(interviewCountLimitString) ? (int?)null : int.Parse(interviewCountLimitString);

            var readSideSettings = new ReadSideSettings(
                WebConfigurationManager.AppSettings["ReadSide.Version"].ParseIntOrNull() ?? 0);

            kernel.Load(
                eventStoreModule,
                new SurveyManagementSharedKernelModule(basePath, isDebug,
                    applicationBuildVersion, interviewDetailsDataLoaderSettings,
                    readSideSettings,
                    isSupervisorFunctionsEnabled: true,
                    interviewLimitCount: interviewCountLimit));


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            CreateAndRegisterEventBus(kernel);

            ServiceLocator.Current.GetInstance<BackgroundSyncronizationTasks>().Configure();


            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<IScheduler>().Start();

            kernel.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();

            kernel.Bind<ITokenVerifier>().To<ApiValidationAntiForgeryTokenVerifier>().InSingletonScope();
            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>();

            

            return kernel;
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            var eventBusConfigSection =
               (EventBusConfigSection)WebConfigurationManager.GetSection("eventBus");

            var bus = new NcqrCompatibleEventDispatcher(kernel.Get<IEventStore>(),
                 eventBusConfigSection.GetSettings(),
                kernel.Get<ILogger>());

            bus.TransactionManager = kernel.Get<ITransactionManagerProvider>();
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