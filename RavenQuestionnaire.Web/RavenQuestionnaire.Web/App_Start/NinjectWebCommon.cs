using System.Web.Mvc;
using Questionnaire.Core.Web.Binding;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;

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
            var kernel = new StandardKernel(new CoreRegistry(System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]));

         //   RegisterServices(kernel);

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);
            //   kernel.Bind<MembershipProvider>().ToConstant(Membership.Provider);
            //  kernel.Inject(Membership.Provider);
            KernelLocator.SetKernel(kernel);
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
            kernel.Bind<IFormsAuthentication>().To<FormsAuthentication>();
            kernel.Bind<IBagManager>().To<ViewBagManager>();
            kernel.Bind<IGlobalInfoProvider>().To<GlobalInfoProvider>();
        }        
    }
}
