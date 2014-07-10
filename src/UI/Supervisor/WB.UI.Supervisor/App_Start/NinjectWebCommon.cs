using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Main.Core;
using Main.Core.Commands;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Extensions.Quartz;
using Ninject.Web.Common;
using Quartz;
using Questionnaire.Core.Web.Binding;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.EventBus;
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
using WB.UI.Shared.Web.Extensions;
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

            bool isEmbeded;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.IsEmbeded"], out isEmbeded))
            {
                isEmbeded = false;
            }

            string storePath = isEmbeded
                ? WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"]
                : WebConfigurationManager.AppSettings["Raven.DocumentStore"];

            int? pageSize = GetEventStorePageSize();

            var ravenSettings = new RavenConnectionSettings(storePath, isEmbedded: isEmbeded,
                username: WebConfigurationManager.AppSettings["Raven.Username"],
                password: WebConfigurationManager.AppSettings["Raven.Password"],
                eventsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"]);

            var schedulerSettings = new SchedulerSettings(LegacyOptions.SchedulerEnabled,
                int.Parse(WebConfigurationManager.AppSettings["Scheduler.HqSynchronizationInterval"]));

            var baseHqUrl = new Uri(WebConfigurationManager.AppSettings["Headquarters.BaseUrl"]);
            var headquartersSettings = new HeadquartersSettings(
                new Uri(baseHqUrl, WebConfigurationManager.AppSettings["Headquarters.LoginServiceEndpoint"]),
                new Uri(baseHqUrl, WebConfigurationManager.AppSettings["Headquarters.UserChangedFeed"]),
                new Uri(baseHqUrl, WebConfigurationManager.AppSettings["Headquarters.InterviewsFeed"]),
                new Uri(baseHqUrl, WebConfigurationManager.AppSettings["Headquarters.QuestionnaireDetailsEndpoint"]).ToString(),
                WebConfigurationManager.AppSettings["Headquarters.AccessToken"],
                new Uri(baseHqUrl, WebConfigurationManager.AppSettings["Headquarters.InterviewsPushEndpoint"]));

            var interviewDetailsDataLoaderSettings =
                new InterviewDetailsDataLoaderSettings(LegacyOptions.SchedulerEnabled,
                    LegacyOptions.InterviewDetailsDataSchedulerSynchronizationInterval,
                    LegacyOptions.InterviewDetailsDataSchedulerNumberOfInterviewsProcessedAtTime);

            bool useStreamingForAllEvents;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.UseStreamingForAllEvents"], out useStreamingForAllEvents))
            {
                useStreamingForAllEvents = true;
            }

            Func<bool> isDebug = () => AppSettings.IsDebugBuilded || HttpContext.Current.IsDebuggingEnabled;
            Version applicationBuildVersion = typeof (AccountController).Assembly.GetName().Version;

            var synchronizationSettings = new SyncSettings(reevaluateInterviewWhenSynchronized: true,
                appDataDirectory: AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                incomingCapiPackagesDirectoryName: LegacyOptions.SynchronizationIncomingCapiPackagesDirectory,
                incomingCapiPackagesWithErrorsDirectoryName:
                    LegacyOptions.SynchronizationIncomingCapiPackagesWithErrorsDirectory,
                incomingCapiPackageFileNameExtension: LegacyOptions.SynchronizationIncomingCapiPackageFileNameExtension);

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new NLogLoggingModule(AppDomain.CurrentDomain.BaseDirectory),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: true),
                new ExpressionProcessorModule(),
                new QuestionnaireVerificationModule(),
                pageSize.HasValue
                    ? new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents, pageSize.Value)
                    : new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents),
                new RavenReadSideInfrastructureModule(ravenSettings, typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly),
                new RavenPlainStorageInfrastructureModule(ravenSettings),
                new FileInfrastructureModule(),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(synchronizationSettings),
                new SurveyManagementWebModule(),
                new QuestionnaireUpgraderModule(),
                new SurveyManagementSharedKernelModule(
                    AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Major"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Minor"]),
                    int.Parse(WebConfigurationManager.AppSettings["SupportedQuestionnaireVersion.Patch"]),
                    isDebug,
                    applicationBuildVersion,
                    interviewDetailsDataLoaderSettings),
                new SupervisorBoundedContextModule(headquartersSettings, schedulerSettings));


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            PrepareNcqrsInfrastucture(kernel);

            var repository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(),
                NcqrsEnvironment.Get<IAggregateSnapshotter>());
            kernel.Bind<IDomainRepository>().ToConstant(repository);
            kernel.Bind<ISnapshotStore>().ToConstant(NcqrsEnvironment.Get<ISnapshotStore>());

            ServiceLocator.Current.GetInstance<BackgroundSyncronizationTasks>().Configure();


            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<IScheduler>().Start();

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

            foreach (var handler in kernel.GetAll(typeof (IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }
        }

        private static int? GetEventStorePageSize()
        {
            int pageSize;

            if (int.TryParse(WebConfigurationManager.AppSettings["EventStorePageSize"], out pageSize))
                return pageSize;
            else
                return null;
        }
    }
}