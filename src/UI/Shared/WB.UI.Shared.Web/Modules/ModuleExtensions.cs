using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject(this IModule module)
        {
            return new NinjectModuleAdapter(module);
        }
    }
}