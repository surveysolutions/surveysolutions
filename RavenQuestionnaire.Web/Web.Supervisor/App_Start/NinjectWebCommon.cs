using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Main.Core;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using Ninject.Web.Common;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Helpers;
using WB.Core.Questionnaire.ImportService;
using WB.Core.Questionnaire.ImportService.Commands;
using Web.Supervisor.App_Start;
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
            var kernel = new StandardKernel(new SupervisorCoreRegistry(storePath, isEmbeded, isApprovedSended));

            kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            NcqrsInit.Init(/*WebConfigurationManager.AppSettings["Raven.DocumentStore"],*/ kernel);

            #warning Nastya: invent a new way of domain service registration
            var commandService = NcqrsEnvironment.Get<ICommandService>() as CommandService;
            commandService.RegisterExecutor(typeof(ImportQuestionnaireCommand), new DefaultImportService());
            // SuccessMarker.Start(kernel);
            return kernel;
        }

    }
}