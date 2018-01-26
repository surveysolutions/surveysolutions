using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace CoreTester.Setup
{
    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject<TModule>(this TModule module)
            where TModule : IModule
        {
            return new NinjectModuleAdapter<TModule>(module);
        }
    }
}