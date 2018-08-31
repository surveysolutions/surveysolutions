using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public static class NinjectModuleExtensions
    {
        public static NinjectModule AsNinject<TModule>(this TModule module)
            where TModule : IModule
        {
            return new NinjectModuleAdapter<TModule>(module);
        }

        public static NinjectModule AsWebNinject<TModule>(this TModule module)
            where TModule : IWebModule
        {
            return new NinjectWebModuleAdapter<TModule>(module);
        }
    }
}
