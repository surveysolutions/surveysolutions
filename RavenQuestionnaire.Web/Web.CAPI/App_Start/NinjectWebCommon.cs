using Web.CAPI.App_Start;

using WebActivator;

[assembly: PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace Web.CAPI.App_Start
{
    using System;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Mvc;
    
    using Main.Core;

    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    using NinjectAdapter;

    using Questionnaire.Core.Web.Binding;
    using Questionnaire.Core.Web.Helpers;

    using SynchronizationMessages.WcfInfrastructure;

    using Web.CAPI.Injections;

    /// <summary>
    /// The ninject web common.
    /// </summary>
    public static class NinjectWebCommon
    {
        #region Constants and Fields

        /// <summary>
        /// The bootstrapper.
        /// </summary>
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        #endregion

        #region Public Methods and Operators

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

        #endregion

        #region Methods

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>
        /// The created kernel.
        /// </returns>
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

            var kernel = new StandardKernel(new CAPICoreRegistry(storePath, isEmbeded));

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();

            NcqrsInit.Init(kernel);
            // SuccessMarker.Start(kernel);
            return kernel;
        }

        #endregion
    }
}