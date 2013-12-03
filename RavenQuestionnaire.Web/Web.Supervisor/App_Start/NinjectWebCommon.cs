using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Core.Supervisor.Denormalizer;
using Core.Supervisor.Views;
using Main.Core;
using Main.Core.Commands;
using Main.Core.Documents;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Web.Common;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.QuestionnaireVerification;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.App_Start;
using Web.Supervisor.Code.CommandDeserialization;
using Web.Supervisor.Injections;
using WebActivator;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace Web.Supervisor.App_Start
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
            SuccessMarker.Stop();
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

            bool isApprovedSended;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["IsApprovedSended"], out isApprovedSended))
            {
                isApprovedSended = false;
            }
            string username = WebConfigurationManager.AppSettings["Raven.Username"];
            string password = WebConfigurationManager.AppSettings["Raven.Password"];

            string defaultDatabase = WebConfigurationManager.AppSettings["Raven.DefaultDatabase"];

            int? pageSize = GetEventStorePageSize();

            var ravenSettings = new RavenConnectionSettings(storePath, isEmbedded: isEmbeded, username: username,
                                                            password: password, defaultDatabase: defaultDatabase);

            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new ServiceLocationModule(),
                new NLogLoggingModule(),
                new DataCollectionSharedKernelModule(),
                new ExpressionProcessorModule(),
                new QuestionnaireVerificationModule(),
                pageSize.HasValue
                    ? new RavenWriteSideInfrastructureModule(ravenSettings, pageSize.Value)
                    : new RavenWriteSideInfrastructureModule(ravenSettings),
                new RavenReadSideInfrastructureModule(ravenSettings, typeof(SupervisorReportsSurveysAndStatusesGroupByTeamMember).Assembly),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(AppDomain.CurrentDomain.GetData("DataDirectory").ToString()),
                new SupervisorCommandDeserializationModule(),
                new SupervisorBoundedContextModule());


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            PrepareNcqrsInfrastucture(kernel);

            var repository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            kernel.Bind<IDomainRepository>().ToConstant(repository);
            kernel.Bind<ISnapshotStore>().ToConstant(NcqrsEnvironment.Get<ISnapshotStore>());
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
            var bus = new EventDispatcher(NcqrsEnvironment.Get<IEventStore>());
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