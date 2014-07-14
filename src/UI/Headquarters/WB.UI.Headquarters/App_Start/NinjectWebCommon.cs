using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
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
using Ninject.Web.WebApi.FilterBindingSyntax;
using Quartz;
using Questionnaire.Core.Web.Binding;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Raven;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.QuestionnaireUpgrader;
using WB.Core.SharedKernels.QuestionnaireVerification;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.Synchronization;
using WB.UI.Headquarters;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Injections;
using WB.UI.Shared.Web.Extensions;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

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
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
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

            bool isApprovedSended;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["IsApprovedSended"], out isApprovedSended))
            {
                isApprovedSended = false;
            }

            int? pageSize = GetEventStorePageSize();

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof (SyncController).Assembly.GetName().Version;

            var ravenSettings = new RavenConnectionSettings(storePath, isEmbeded, WebConfigurationManager.AppSettings["Raven.Username"],
                WebConfigurationManager.AppSettings["Raven.Password"], WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"],
                activeBundles: WebConfigurationManager.AppSettings["Raven.Databases.ActiveBundles"]);

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled && LegacyOptions.SupervisorFunctionsEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                    LegacyOptions.InterviewDetailsDataSchedulerNumberOfInterviewsProcessedAtTime);
            
            bool useStreamingForAllEvents;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.UseStreamingForAllEvents"], out useStreamingForAllEvents))
            {
                useStreamingForAllEvents = true;
            }

            bool reevaluateInterviewWhenSynchronized = LegacyOptions.SupervisorFunctionsEnabled;
            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: reevaluateInterviewWhenSynchronized,
                appDataDirectory: AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                incomingCapiPackagesDirectoryName: LegacyOptions.SynchronizationIncomingCapiPackagesDirectory,
                incomingCapiPackagesWithErrorsDirectoryName:
                    LegacyOptions.SynchronizationIncomingCapiPackagesWithErrorsDirectory,
                incomingCapiPackageFileNameExtension: LegacyOptions.SynchronizationIncomingCapiPackageFileNameExtension);

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false),
                new ExpressionProcessorModule(),
                new QuestionnaireVerificationModule(),
                new QuestionnaireUpgraderModule(),
                pageSize.HasValue
                    ? new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents, pageSize.Value)
                    : new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents),
                new RavenReadSideInfrastructureModule(ravenSettings, typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly),
                new RavenPlainStorageInfrastructureModule(ravenSettings),
                new FileInfrastructureModule(),
                new HeadquartersRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new HeadquartersBoundedContextModule(LegacyOptions.SupervisorFunctionsEnabled),
                new SurveyManagementSharedKernelModule(
                    AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Major"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Minor"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Patch"]),
                    isDebug,
                    applicationBuildVersion,
                    interviewDetailsDataLoaderSettings));


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            PrepareNcqrsInfrastucture(kernel);

            var repository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(),
                NcqrsEnvironment.Get<IAggregateSnapshotter>());
            kernel.Bind<IDomainRepository>().ToConstant(repository);
            kernel.Bind<ISnapshotStore>().ToConstant(NcqrsEnvironment.Get<ISnapshotStore>());

            kernel.Bind<ITokenVerifier>().ToConstant(new SimpleTokenVerifier(WebConfigurationManager.AppSettings["Synchronization.Key"]));

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .WhenControllerHas<TokenValidationAuthorizationAttribute>();

            kernel.BindHttpFilter<HeadquarterFeatureOnlyFilter>(System.Web.Http.Filters.FilterScope.Controller)
               .WhenControllerHas<HeadquarterFeatureOnlyAttribute>();

            kernel.Bind<IIdentityManager>().To<IdentityManager>().InSingletonScope();

            if (LegacyOptions.SupervisorFunctionsEnabled)
            {
                ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            }

            ServiceLocator.Current.GetInstance<IScheduler>().Start();

#warning dirty index registrations
            // SuccessMarker.Start(kernel);
            return kernel;
        }

        private static void PrepareNcqrsInfrastucture(StandardKernel kernel)
        {
            var commandService = new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.SetDefault(commandService);
            NcqrsInit.InitializeCommandService(kernel.Get<ICommandListSupplier>(), commandService);
            kernel.Bind<ICommandService>().ToConstant(commandService);
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());

            CreateAndRegisterEventBus(kernel);
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            var bus = new NcqrCompatibleEventDispatcher();
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);
            foreach (object handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register((IEventHandler)handler);
            }
        }

        private static int? GetEventStorePageSize()
        {
            int pageSize;

            if (int.TryParse(WebConfigurationManager.AppSettings["EventStorePageSize"], out pageSize))
                return pageSize;
            return null;
        }
    }
}