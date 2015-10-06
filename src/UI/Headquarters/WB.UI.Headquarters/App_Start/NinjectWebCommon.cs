using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Main.DenormalizerStorage;
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
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
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
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Core.Synchronization;
using WB.UI.Headquarters;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Injections;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WebActivatorEx;
using FilterScope = System.Web.Http.Filters.FilterScope;

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
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            Global.Initialize(); // pinging global.asax to perform it's part of static initialization

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;

            Version applicationBuildVersion = typeof(SyncController).Assembly.GetName().Version;

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval);

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
                retryIntervalInSeconds: LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval);

            var basePath = appDataDirectory;
            //const string QuestionnaireAssembliesFolder = "QuestionnaireAssemblies";

            var mappingAssemblies = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly};
            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = WebConfigurationManager.ConnectionStrings["PlainStore"].ConnectionString,
                MappingAssemblies = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly, typeof(HeadquartersBoundedContextModule).Assembly, typeof(SynchronizationModule).Assembly }
            };

            int postgresCacheSize = WebConfigurationManager.AppSettings["Postgres.CacheSize"].ParseIntOrNull() ?? 1024;

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule(),
                new NLogLoggingModule(),
                new SurveyManagementDataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false, basePath: basePath),
                new QuestionnaireUpgraderModule(),
                new PostgresPlainStorageModule(postgresPlainStorageSettings),
                new FileInfrastructureModule(),
                new HeadquartersRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),

                new PostresKeyValueModule(postgresCacheSize),
                new PostgresReadSideModule(WebConfigurationManager.ConnectionStrings["ReadSide"].ConnectionString, postgresCacheSize, mappingAssemblies));

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

            kernel.Load(
                eventStoreModule,
                new SurveyManagementSharedKernelModule(basePath, isDebug,
                    applicationBuildVersion, interviewDetailsDataLoaderSettings, true,
                    int.Parse(WebConfigurationManager.AppSettings["Export.MaxCountOfCachedEntitiesForSqliteDb"]),
                    new InterviewDataExportSettings(basePath,
                        bool.Parse(WebConfigurationManager.AppSettings["Export.EnableInterviewHistory"]),
                        WebConfigurationManager.AppSettings["Export.MaxRecordsCountPerOneExportQuery"].ToInt(10000)),
                    LegacyOptions.SupervisorFunctionsEnabled,
                    interviewCountLimit),
                new HeadquartersBoundedContextModule(LegacyOptions.SupervisorFunctionsEnabled, userPreloadingSettings));


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

            kernel.BindHttpFilter<HeadquarterFeatureOnlyFilter>(FilterScope.Controller)
               .WhenControllerHas<HeadquarterFeatureOnlyAttribute>();

            kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<UserPreloadingVerificationTask>().Configure();
            ServiceLocator.Current.GetInstance<UserBatchCreatingTask>().Configure();
            ServiceLocator.Current.GetInstance<UserPreloadingCleanerTask>().Configure();

            ServiceLocator.Current.GetInstance<IScheduler>().Start();

            kernel.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();

            return kernel;
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            var ignoredDenormalizersConfigSection =
                (IgnoredDenormalizersConfigSection) WebConfigurationManager.GetSection("IgnoredDenormalizersSection");
            Type[] handlersToIgnore = ignoredDenormalizersConfigSection == null ? new Type[0] : ignoredDenormalizersConfigSection.GetIgnoredTypes();
            var bus = new NcqrCompatibleEventDispatcher(kernel.Get<IEventStore>(), handlersToIgnore);
            bus.TransactionManager = kernel.Get<ITransactionManagerProvider>();
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<ILiteEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);
            foreach (object handler in kernel.GetAll(typeof(IEventHandler)))
            {
                bus.Register((IEventHandler)handler);
            }
        }
    }
}