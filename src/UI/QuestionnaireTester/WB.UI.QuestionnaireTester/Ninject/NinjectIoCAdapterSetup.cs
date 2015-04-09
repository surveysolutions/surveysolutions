using Cirrious.CrossCore.IoC;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(
                new SecurityModule(), 
                new NetworkModule(), 
                new LoggerModule(),
                new ApplicationModule(),
                new ServiceLocationModule(),
                new PlainStorageInfrastructureModule(),
                new MobileDataCollectionModule(),
                new NinjectModuleAdapter<InfrastructureModuleMobile>(new InfrastructureModuleMobile()));
        }
    }

    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject<TModule>(this TModule module)
            where TModule : IModule
        {
            return new NinjectModuleAdapter<TModule>(module);
        }
    }

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
    }
}