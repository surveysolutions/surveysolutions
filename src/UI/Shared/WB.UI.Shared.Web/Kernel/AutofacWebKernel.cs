using System.Linq;
using Autofac;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.UI.Shared.Web.Modules;

namespace WB.UI.Shared.Web.Kernel
{
    public class AutofacWebKernel : AutofacKernel
    {
        public void Load(params IWebModule[] modules)
        {
            var autofacModules = modules.Select(module => module.AsWebAutofac()).ToArray();
            foreach (var autofacModule in autofacModules)
            {
                this.containerBuilder.RegisterModule(autofacModule);
            }
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }
    }
}
