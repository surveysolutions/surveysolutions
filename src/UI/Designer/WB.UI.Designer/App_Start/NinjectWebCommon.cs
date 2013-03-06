using Main.Core;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using System;
using System.Web;
using System.Web.Configuration;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WB.UI.Designer.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(WB.UI.Designer.App_Start.NinjectWebCommon), "PostStart")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(WB.UI.Designer.App_Start.NinjectWebCommon), "Stop")]
namespace WB.UI.Designer.App_Start
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
            //DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
        }

        public static void PostStart()
        {
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
            {
                isEmbeded = false;
            }

            string storePath = isEmbeded
                                   ? WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"]
                                   : WebConfigurationManager.AppSettings["Raven.DocumentStore"];

            var kernel = new StandardKernel(new DesignCoreRegistry( repositoryPath: storePath, isEmbeded: isEmbeded));
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            NcqrsInit.Init(kernel);

            try
            {
                NcqrsInit.RebuildReadLayer(kernel);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
            }
            

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load<MembershipModule>();
        }        
    }
}
