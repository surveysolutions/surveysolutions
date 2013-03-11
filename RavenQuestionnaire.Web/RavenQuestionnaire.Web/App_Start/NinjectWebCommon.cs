using System.Web.Configuration;
using System.Web.Mvc;
using Main.Core;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Web.Injections;

[assembly: WebActivator.PreApplicationStartMethod(typeof(RavenQuestionnaire.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(RavenQuestionnaire.Web.App_Start.NinjectWebCommon), "Stop")]

namespace RavenQuestionnaire.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    using NinjectAdapter;

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
            SuccessMarker.Stop();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            bool isEmbeded;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.IsEmbeded"], out isEmbeded))
            {
                isEmbeded = false;
            }

            string storePath = isEmbeded
                                   ? WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"]
                                   : WebConfigurationManager.AppSettings["Raven.DocumentStore"];
            var kernel = new StandardKernel(new MainCoreRegistry(storePath, isEmbeded));

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            NcqrsInit.Init(/*WebConfigurationManager.AppSettings["Raven.DocumentStore"],*/ kernel);
            // SuccessMarker.Start(kernel);
            return kernel;
        }
      
    }
}
