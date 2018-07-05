using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.ResolveAnything;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class AutofacKernel : IKernel
    {
        public AutofacKernel()
        {
            this.containerBuilder = new ContainerBuilder();
        }

        private readonly ContainerBuilder containerBuilder;
        private readonly List<IInitModule> initModules = new List<IInitModule>();

        public IContainer Container { get; set; }

        public void Load(params IModule[] modules)
        {
            var autofacModules = modules.Select(module => module.AsAutofac()).ToArray();
            foreach (var autofacModule in autofacModules)
            {
                this.containerBuilder.RegisterModule(autofacModule);
            }
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }

        public async Task Init()
        {
            containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            Container = containerBuilder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            foreach (var module in initModules)
            {
                await module.Init(ServiceLocator.Current); 
            }
        }
    }
}
