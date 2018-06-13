using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Ninject;
using Ninject.Web.Common;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Ioc;

using IKernel = Ninject.IKernel;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectKernel : WB.Core.Infrastructure.Modularity.IKernel
    {
        public NinjectKernel()
        {
            var ninjectSettings = new NinjectSettings { InjectNonPublic = true };
            this.Kernel = new StandardKernel(ninjectSettings);
        }

        public IKernel Kernel { get; set; }
        private readonly List<IInitModule> initModules = new List<IInitModule>();

        public void Load(params IModule[] modules)
        {
            var ninjectModules = modules.Select(module => module.AsNinject()).ToArray();
            this.Kernel.Load(ninjectModules);
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }

        public void Load(params IWebModule[] modules)
        {
            var ninjectModules = modules.Select(module => module.AsWebNinject()).ToArray();
            this.Kernel.Load(ninjectModules);
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }

        public async Task Init()
        {
            this.Kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            this.Kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            // ServiceLocator
            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            foreach (var module in initModules)
            {
                await module.Init(ServiceLocator.Current);
            }
        }
    }
}
