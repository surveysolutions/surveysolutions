using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectModuleAdapter : NinjectModule, IIocRegistry
    {
        private readonly IModule module;

        public NinjectModuleAdapter(IModule module)
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
    }
}