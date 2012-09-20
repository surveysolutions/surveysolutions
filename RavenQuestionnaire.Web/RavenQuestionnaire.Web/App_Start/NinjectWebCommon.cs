using System.ServiceModel;
using System.Threading;
using System.Web.Configuration;
using System.Web.Mvc;
using Main.Core;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Raven.Client;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using Main.Core.Events;
using RavenQuestionnaire.Web.Injections;
using RavenQuestionnaire.Web.Synchronization;

[assembly: WebActivator.PreApplicationStartMethod(typeof(RavenQuestionnaire.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(RavenQuestionnaire.Web.App_Start.NinjectWebCommon), "Stop")]

namespace RavenQuestionnaire.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
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
            var kernel = new StandardKernel(new MainCoreRegistry(storePath, isEmbeded));

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            //   kernel.Bind<MembershipProvider>().ToConstant(Membership.Provider);
            //  kernel.Inject(Membership.Provider);
            KernelLocator.SetKernel(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<IExportImport>().To<ExportImportEvent>();
            kernel.Bind<IEventSync>().To<HQEventSync>();
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

            kernel.Bind<IDocumentSession>().ToMethod(
               context => context.Kernel.Get<DocumentStore>().OpenSession()).When(
                   b => HttpContext.Current != null).InScope(
                       o => HttpContext.Current);

            kernel.Bind<IDocumentSession>().ToMethod(
            context => context.Kernel.Get<DocumentStore>().OpenSession()).When(
                b => OperationContext.Current != null).InScope(o => OperationContext.Current);

            kernel.Bind<IDocumentSession>().ToMethod(
                context => context.Kernel.Get<DocumentStore>().OpenSession()).When(
                    b => HttpContext.Current == null && OperationContext.Current == null).InScope(o => Thread.CurrentThread);
        }
      
    }
}
