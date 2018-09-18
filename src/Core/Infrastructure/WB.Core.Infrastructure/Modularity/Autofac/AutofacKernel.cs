using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        protected readonly ContainerBuilder containerBuilder;
        protected readonly List<IInitModule> initModules = new List<IInitModule>();

        public ContainerBuilder ContainerBuilder => containerBuilder;

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

        
        public Task Init()
        {
            this.containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            this.containerBuilder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>().InstancePerLifetimeScope();

            var status = new UnderConstructionInfo();
            this.containerBuilder.Register((ctx, p) => status).SingleInstance();

            Container = containerBuilder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            Thread thread = new Thread(() => InitModules(status).Wait()) { IsBackground = false };
            thread.Start();

            return Task.CompletedTask;
        }

        private async Task InitModules(UnderConstructionInfo status)
        {
            status.Run();

            try
            {
                foreach (var module in initModules)
                    {
                        status.ClearMessage();
                        await module.Init(ServiceLocator.Current, status);
                    }
            }
            finally
            {
                status.Finish();
            }
        }
    }
}
