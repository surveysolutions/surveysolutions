using Ninject;
using Ninject.Activation;
using Ninject.Syntax;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectModuleContext : IModuleContext
    {
        private readonly IContext context;

        public NinjectModuleContext(IContext context)
        {
            this.context = context;
        }

        public T Resolve<T>()
        {
            return context.Kernel.Get<T>();
        }
    }

    public class NinjectConstructorContext : IConstructorContext
    {
        private readonly IConstructorArgumentSyntax context;

        public NinjectConstructorContext(IConstructorArgumentSyntax context)
        {
            this.context = context;
        }

        public T Inject<T>()
        {
            return context.Inject<T>();
        }
    }
}