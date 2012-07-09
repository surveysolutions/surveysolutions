using Ninject;
using Raven.Client;
using System.ServiceModel;
using RavenQuestionnaire.Core;
using System.Collections.Concurrent;

namespace QApp
{
    public static class Initializer
    {

        #region Properties

        public static IKernel Kernel { get; private set; }
        private static ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        #endregion

        #region Method

        public static void Init()
        {
            Kernel = CreateKernel();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new CoreRegistry("http://localhost:8080", false));
            RegisterServices(kernel);
            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDocumentSession>().ToMethod(
              context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                  b => OperationContext.Current == null).InSingletonScope();
            kernel.Bind<IDocumentSession>().ToMethod(
                context => context.Kernel.Get<IDocumentStore>().OpenSession()).When(
                 b => OperationContext.Current != null).InScope(o => OperationContext.Current);
        }

        #endregion

        //private static void RegisterServices(IKernel kernel)
        //{
        //    kernel.Scan(s =>
        //    {
        //        s.FromAssembliesMatching("RavenQuestionnaire.*");
        //        s.BindWith(new GenericBindingGenerator(typeof(ICommandHandler<>)));
        //    });
        //    kernel.Scan(s =>
        //    {
        //        s.FromAssembliesMatching("RavenQuestionnaire.*");
        //        s.BindWith(new GenericBindingGenerator(typeof(IViewFactory<,>)));
        //    });
        //    kernel.Scan(s =>
        //    {
        //        s.FromAssembliesMatching("RavenQuestionnaire.*");
        //        s.BindWith(new GenericBindingGenerator(typeof(IExpressionExecutor<,>)));
        //    });
        //    kernel.Scan(s =>
        //    {
        //        s.FromAssembliesMatching("RavenQuestionnaire.*");
        //        s.BindWith(new RegisterFirstInstanceOfInterface());
        //    });
        //    kernel.Scan(s =>
        //    {
        //        s.FromAssembliesMatching("RavenQuestionnaire.*");
        //        s.BindWith(new GenericBindingGenerator(typeof(Iterator<>)));
        //    });
        //    //kernel.Bind<IGlobalInfoProvider>().To<GlobalInfoProvider>();
        //}   
    }
}
