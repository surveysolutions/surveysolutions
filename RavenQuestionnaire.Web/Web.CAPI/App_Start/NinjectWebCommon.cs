using System.Collections.Concurrent;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Core.CAPI.Synchronization;
using DataEntryClient.WcfInfrastructure;
using Main.Core;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Raven.Client;
using Raven.Client.Document;
using Main.Core.Events;
using Web.CAPI.Injections;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Web.CAPI.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Web.CAPI.App_Start.NinjectWebCommon), "Stop")]

namespace Web.CAPI.App_Start
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
                isEmbeded = false;
            string storePath;
            if (isEmbeded)
                storePath = WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"];
            else
                storePath = WebConfigurationManager.AppSettings["Raven.DocumentStore"];
            var kernel = new StandardKernel(new CAPICoreRegistry(storePath, isEmbeded));

            //   RegisterServices(kernel);

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            //   kernel.Bind<MembershipProvider>().ToConstant(Membership.Provider);
            //  kernel.Inject(Membership.Provider);
            KernelLocator.SetKernel(kernel);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();
            NCQRSInit.Init( /*System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"], */
                kernel);
            SuccessMarker.Start(kernel);
            return kernel;
        }

    }
}
