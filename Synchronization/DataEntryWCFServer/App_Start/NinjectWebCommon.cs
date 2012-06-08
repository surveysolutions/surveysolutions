using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Raven.Client;
using RavenQuestionnaire.Core;

[assembly: WebActivator.PreApplicationStartMethod(typeof(DataEntryWCFServer.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(DataEntryWCFServer.App_Start.NinjectWebCommon), "Stop")]

namespace DataEntryWCFServer.App_Start
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
            var kernel = new StandardKernel(new CoreRegistry(System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]));
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }
        private static ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDocumentSession>().ToMethod(
              context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                  b => OperationContext.Current != null).InScope(o => OperationContext.Current);
            
            kernel.Bind<IDocumentSession>().ToMethod(
                context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                    b => OperationContext.Current == null).InScope(o => Thread.CurrentThread);
        
        }        
    }
}
