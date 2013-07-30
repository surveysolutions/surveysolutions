using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Core.Supervisor.Denormalizer;
using Core.Supervisor.RavenIndexes;
using Core.Supervisor.Views;
using Main.Core;
using Main.Core.Documents;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using Ninject.Web.Common;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging.NLog;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Raven;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.App_Start;
using Web.Supervisor.CommandDeserialization;
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

            string defaultDatabase  = WebConfigurationManager.AppSettings["Raven.DefaultDatabase"];

            int? pageSize = GetEventStorePageSize();

            var ravenSettings = new RavenConnectionSettings(storePath, isEmbedded: isEmbeded, username: username, password: password, defaultDatabase: defaultDatabase);

            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                new ServiceLocationModule(),
                new NLogLoggingModule(),
                pageSize.HasValue
                    ? new RavenWriteSideInfrastructureModule(ravenSettings, pageSize.Value)
                    : new RavenWriteSideInfrastructureModule(ravenSettings),
                new RavenReadSideInfrastructureModule(ravenSettings),
                new SupervisorCoreRegistry(),
                new SynchronizationModule(AppDomain.CurrentDomain.GetData("DataDirectory").ToString()),
                new SupervisorCommandDeserializationModule());

#warning dirty hack for register ziped read side
            kernel.Unbind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>();
            kernel.Unbind<IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>();

            //midnigth fixx
            //both services have to share the same cache
            //they have to have two different implementations and _maybe_ share single cache
            kernel.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>, IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>()
                .To<RavenReadSideRepositoryWriterWithCacheAndZip<CompleteQuestionnaireStoreDocument>>().InSingletonScope();
            
            //kernel.Bind<IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>().To<RavenReadSideRepositoryWriterWithCacheAndZip<CompleteQuestionnaireStoreDocument>>().InSingletonScope();


            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();


            NcqrsInit.Init(kernel);

            kernel.Bind<ICommandService>().ToConstant(NcqrsEnvironment.Get<ICommandService>());


#warning dirty index registrations
            var indexccessor = kernel.Get<IReadSideRepositoryIndexAccessor>();
            indexccessor.RegisterIndexesFormAssembly(typeof(SummaryItemByTemplate).Assembly);
            // SuccessMarker.Start(kernel);
            return kernel;
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