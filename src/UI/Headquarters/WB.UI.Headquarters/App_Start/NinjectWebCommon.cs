using System.Web.Configuration;
using System.Web.Http;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using NConfig;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Mvc;
using System;
using System.Web;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.Infrastructure.Raven;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WB.UI.Headquarters.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(WB.UI.Headquarters.App_Start.NinjectWebCommon), "Stop")]

namespace WB.UI.Headquarters.App_Start
{


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
            MvcApplication.Initialize();
            var ravenConnectionSettings = new RavenConnectionSettings(
                WebConfigurationManager.AppSettings["Raven.DocumentStore"], 
                isEmbedded: false,
                username: WebConfigurationManager.AppSettings["Raven.Username"],
                password: WebConfigurationManager.AppSettings["Raven.Password"],
                eventsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"]);

            var kernel = new StandardKernel(
                new RavenPlainStorageInfrastructureModule(ravenConnectionSettings),
                new AuthenticationModule(),
                new HeadquartersBoundedContextModule());

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
        }        
    }
}
