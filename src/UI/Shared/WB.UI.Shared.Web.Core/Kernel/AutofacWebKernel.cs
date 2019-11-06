using System;
using System.Linq;
using Autofac;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.UI.Shared.Web.Modules;

namespace WB.UI.Shared.Web.Kernel
{
    public class AutofacWebKernel : AutofacKernel
    {
        public override void Load<T>(params IModule<T>[] modules)
        {
            var autofacModules = modules.Select(module =>
            {
                switch (module)
                {
                    case IWebModule webModule: return webModule.AsWebAutofac();
                    case IModule iModule: return iModule.AsAutofac();
                    default:
                        throw new ArgumentException("Cant resolve module type: " + module.GetType());
                }
            }).ToArray();

            foreach (var autofacModule in autofacModules)
            {
                this.containerBuilder.RegisterModule(autofacModule);
            }

            initModules.AddRange(modules);
        }
    }
}
