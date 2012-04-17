using Ninject;
using Ninject.Extensions.Conventions;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.ExpressionExecutors;


namespace QApp
{
    public static class Initializer
    {
        public static void Init()
        {
            Kernel = CreateKernel();
        }

        public static IKernel Kernel { get; private set; }
   

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new CoreRegistry("http://localhost:8080", false));//add settings reading or embedded mode

            RegisterServices(kernel);

            //KernelLocator.SetKernel(kernel);
            return kernel;
        }



        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(ICommandHandler<>)));
            });

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IViewFactory<,>)));
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IExpressionExecutor<,>)));
            });

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new RegisterFirstInstanceOfInterface());
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(Iterator<>)));
            });

            
            //kernel.Bind<IGlobalInfoProvider>().To<GlobalInfoProvider>();
        }   
    }
}
