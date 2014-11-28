using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Android
{
    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject(this IModule module)
        {
            return new NinjectModuleAdapter(module);
        }
    }
}