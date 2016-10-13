using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.WebApi.FilterBindingSyntax;
using Quartz;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Core.Synchronization;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Implementation.Services;
using WB.UI.Headquarters.Injections;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Shared.Web.Versions;
using WebActivatorEx;
using FilterScope = System.Web.Http.Filters.FilterScope;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace WB.UI.Headquarters
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
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            Global.Initialize(); // pinging global.asax to perform it's part of static initialization

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                    synchronizationBatchCount: WebConfigurationManager.AppSettings.GetInt("Scheduler.SynchronizationBatchCount", @default: 5),
                    synchronizationParallelExecutorsCount: WebConfigurationManager.AppSettings.GetInt("Scheduler.SynchronizationParallelExecutorsCount", @default: 1));

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
                origin: Constants.SupervisorSynchronizationOrigin, 
                retryCount: int.Parse(WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.RetryCount"]),
                retryIntervalInSeconds: LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                useBackgroundJobForProcessingPackages: WebConfigurationManager.AppSettings.GetBool("Synchronization.UseBackgroundJobForProcessingPackages", @default: false));

            var basePath = appDataDirectory;
            //const string QuestionnaireAssembliesFolder = "QuestionnaireAssemblies";

            var mappingAssemblies = new List<Assembly> { typeof(HeadquartersBoundedContextModule).Assembly };
            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = WebConfigurationManager.ConnectionStrings["PlainStore"].ConnectionString,
                DbUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                }
            };

            var cacheSettings = new ReadSideCacheSettings(
                enableEsentCache: WebConfigurationManager.AppSettings.GetBool("Esent.Cache.Enabled", @default: true),
                esentCacheFolder: Path.Combine(appDataDirectory, WebConfigurationManager.AppSettings.GetString("Esent.Cache.Folder", @default: @"Temp\EsentCache")),
                cacheSizeInEntities: WebConfigurationManager.AppSettings.GetInt("ReadSide.CacheSize", @default: 1024),
                storeOperationBulkSize: WebConfigurationManager.AppSettings.GetInt("ReadSide.BulkSize", @default: 512));

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new QuestionnaireUpgraderModule(),
                new FileInfrastructureModule(),
                new ProductVersionModule(typeof(HeadquartersRegistry).Assembly),
                new HeadquartersRegistry(),
                new PostgresKeyValueModule(cacheSettings),
                new PostgresPlainStorageModule(postgresPlainStorageSettings),
                new PostgresReadSideModule(
                    WebConfigurationManager.ConnectionStrings["ReadSide"].ConnectionString,
                    new DbUpgradeSettings(typeof(M001_InitDb).Assembly, typeof(M001_InitDb).Namespace), 
                    cacheSettings, 
                    mappingAssemblies)
            );

            var eventStoreModule = ModulesFactory.GetEventStoreModule();

            var interviewCountLimitString = WebConfigurationManager.AppSettings["Limits.MaxNumberOfInterviews"];
            int? interviewCountLimit = string.IsNullOrEmpty(interviewCountLimitString) ? (int?)null : int.Parse(interviewCountLimitString);

            var userPreloadingConfigurationSection = (UserPreloadingConfigurationSection)(WebConfigurationManager.GetSection("userPreloadingSettingsGroup/userPreloadingSettings") ?? new UserPreloadingConfigurationSection());

            var userPreloadingSettings =
                new UserPreloadingSettings(
                    userPreloadingConfigurationSection.VerificationIntervalInSeconds,
                    userPreloadingConfigurationSection.CreationIntervalInSeconds,
                    userPreloadingConfigurationSection.CleaningIntervalInHours,
                    userPreloadingConfigurationSection.HowOldInDaysProcessShouldBeInOrderToBeCleaned,
                    userPreloadingConfigurationSection.MaxAllowedRecordNumber,
                    userPreloadingConfigurationSection.NumberOfRowsToBeVerifiedInOrderToUpdateVerificationProgress,
                    userPreloadingConfigurationSection.NumberOfValidationErrorsBeforeStopValidation,
                    loginFormatRegex: UserModel.UserNameRegularExpression,
                    emailFormatRegex: userPreloadingConfigurationSection.EmailFormatRegex,
                    passwordFormatRegex: MembershipProviderSettings.Instance.PasswordStrengthRegularExpression,
                    phoneNumberFormatRegex: userPreloadingConfigurationSection.PhoneNumberFormatRegex);

            var readSideSettings = new ReadSideSettings(
                WebConfigurationManager.AppSettings["ReadSide.Version"].ParseIntOrNull() ?? 0);

            var exportSettings = new ExportSettings(
                WebConfigurationManager.AppSettings["Export.BackgroundExportIntervalInSeconds"].ToIntOrDefault(15));
            var interviewDataExportSettings=
            new InterviewDataExportSettings(basePath,
                bool.Parse(WebConfigurationManager.AppSettings["Export.EnableInterviewHistory"]),
                WebConfigurationManager.AppSettings["Export.MaxRecordsCountPerOneExportQuery"].ToIntOrDefault(10000),
                WebConfigurationManager.AppSettings["Export.LimitOfCachedItemsByDenormalizer"].ToIntOrDefault(100),
                WebConfigurationManager.AppSettings["Export.InterviewsExportParallelTasksLimit"].ToIntOrDefault(10),
                WebConfigurationManager.AppSettings["Export.InterviewIdsQueryBatchSize"].ToIntOrDefault(40000));

            var sampleImportSettings = new SampleImportSettings(
                WebConfigurationManager.AppSettings["PreLoading.InterviewsImportParallelTasksLimit"].ToIntOrDefault(2));

            kernel.Load(
                eventStoreModule,
                new HeadquartersBoundedContextModule(basePath, 
                    interviewDetailsDataLoaderSettings,
                    readSideSettings,
                    userPreloadingSettings, 
                    exportSettings,
                    interviewDataExportSettings, 
                    sampleImportSettings,
                    synchronizationSettings,
                    interviewCountLimit));


            kernel.Bind<ISettingsProvider>().To<SettingsProvider>();

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            CreateAndRegisterEventBus(kernel);

            kernel.Bind<ITokenVerifier>().ToConstant(new SimpleTokenVerifier(WebConfigurationManager.AppSettings["Synchronization.Key"]));

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<TokenValidationAuthorizationAttribute>();

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>()
                .WithConstructorArgument("tokenVerifier", new ApiValidationAntiForgeryTokenVerifier());

            kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<UserPreloadingCleanerTask>().Configure();
            ServiceLocator.Current.GetInstance<ExportJobScheduler>().Configure();

            ServiceLocator.Current.GetInstance<IScheduler>().Start();

            kernel.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();

            kernel.Unbind<IInterviewImportService>();
            kernel.Bind<IInterviewImportService>().To<InterviewImportService>().InSingletonScope();
            kernel.Bind<IRestoreDeletedQuestionnaireProjectionsService>().To<RestoreDeletedQuestionnaireProjectionsService>();

            return kernel;
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            var eventBusConfigSection =
                (EventBusConfigSection)WebConfigurationManager.GetSection("eventBus");

            var bus = new NcqrCompatibleEventDispatcher(
                kernel.Get<IEventStore>(),
                eventBusConfigSection.GetSettings(),
                kernel.Get<ILogger>());

            bus.TransactionManager = kernel.Get<ITransactionManagerProvider>();
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<ILiteEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);
                
            //Kernel.RegisterDenormalizer<>() - should be used instead
            var enumerable = kernel.GetAll(typeof(IEventHandler)).ToList();
            foreach (object handler in enumerable)
            {
                bus.Register((IEventHandler)handler);
            }
        }
    }
}