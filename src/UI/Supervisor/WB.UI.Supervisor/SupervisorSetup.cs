using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using MvvmCross.IoC;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Supervisor.ServiceLocation;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Logging;
using MvxIoCProvider = WB.UI.Shared.Enumerator.Autofac.MvxIoCProvider;

namespace WB.UI.Supervisor
{
    public class SupervisorSetup : EnumeratorSetup<SupervisorMvxApplication>
    {
        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new MvxIoCProvider(this.CreateAndInitializeIoc());
        }

        private IContainer CreateAndInitializeIoc()
        {
            var modules = new IModule[] {
                new NcqrsModule(),
                new InfrastructureModuleMobile(),
                new DataCollectionSharedKernelModule(),
                new EnumeratorUIModule(),
                new EnumeratorSharedKernelModule(),
                new SupervisorUIModule(),
                };

            ContainerBuilder builder = new ContainerBuilder();
            foreach (var module in modules)
            {
                builder.RegisterModule(module.AsAutofac());
            }

            builder.RegisterType<NLogLogger>().As<ILogger>();

            var container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));

            var serviceLocator = ServiceLocator.Current;
            foreach (var module in modules)
            {
                module.Init(serviceLocator).Wait();
            }

            return container;
        }

        protected override IEnumerable<Assembly> AndroidViewAssemblies
        {
            get
            {
                var toReturn = base.AndroidViewAssemblies;

                // Add assemblies with other views we use.  When the XML is inflated
                // MvvmCross knows about the types and won't complain about them.  This
                // speeds up inflation noticeably.
                return toReturn;
            }
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(SupervisorSetup).Assembly
            });
        }
    }
}
