using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Tester.Ninject
{
    public class NinjectModuleAdapter<TModule> : NinjectModule, IIocRegistry
        where TModule : IModule
    {
        private readonly TModule module;

        public NinjectModuleAdapter(TModule module)
        {
            this.module = module;
        }

        public override void Load()
        {
            this.module.Load(this);
        }

        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>();
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>() 
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
        }
    }
}