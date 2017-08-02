using Autofac.Core;
using WB.UI.Shared.Enumerator.Services.Ninject;

namespace WB.UI.Shared.Enumerator.Autofac
{
    public static class ModuleExtensions
    {
        public static IModule AsAutofac(this Core.Infrastructure.Modularity.IModule ourModule)
        {
            return new AutofacModuleAdapter(ourModule);
        }
    }
}