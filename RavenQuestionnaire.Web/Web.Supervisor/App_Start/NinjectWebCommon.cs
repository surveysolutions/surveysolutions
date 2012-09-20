using System;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Core.Supervisor.Synchronization;
using Main.Core;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Raven.Client;
using Raven.Client.Document;
using Main.Core.Events;

using Web.Supervisor.App_Start;
using Web.Supervisor.Injections;
using WebActivator;

[assembly: WebActivator.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace Web.Supervisor.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            bool isEmbeded;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.IsEmbeded"], out isEmbeded))
                isEmbeded = false;
            string storePath;
            if (isEmbeded)
                storePath = WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"];
            else
                storePath = WebConfigurationManager.AppSettings["Raven.DocumentStore"];
            var kernel = new StandardKernel(new SupervisorCoreRegistry(storePath, isEmbeded));
            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            KernelLocator.SetKernel(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<IExportImport>().To<ExportImportEvent>();
            kernel.Bind<IEventSync>().To<SupervisorEventSync>();
            RegisterServices(kernel);
            NCQRSInit.Init(/*WebConfigurationManager.AppSettings["Raven.DocumentStore"],*/ kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDocumentSession>().ToMethod(context => context.Kernel.Get<DocumentStore>().OpenSession()).When( b => HttpContext.Current != null).InScope(o => HttpContext.Current);
            kernel.Bind<IDocumentSession>().ToMethod(context => context.Kernel.Get<DocumentStore>().OpenSession()).When(b => OperationContext.Current != null).InScope(o => OperationContext.Current);
            kernel.Bind<IDocumentSession>().ToMethod(context => context.Kernel.Get<DocumentStore>().OpenSession()).When(b => HttpContext.Current == null && OperationContext.Current == null).InScope(o => Thread.CurrentThread);

        }
    }
}