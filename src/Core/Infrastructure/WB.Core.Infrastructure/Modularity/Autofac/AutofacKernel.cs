using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.ResolveAnything;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Resources;
using WB.Infrastructure.Native;

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

        public virtual void Load<T>(params IModule<T>[] modules) where T: IIocRegistry
        {
            var autofacModules = modules.Select(module =>
            {
                switch (module)
                {
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

        
        public Task InitAsync()
        {
            this.containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            this.containerBuilder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>().InstancePerLifetimeScope();

            var status = new UnderConstructionInfo();
            this.containerBuilder.Register((ctx, p) => status).SingleInstance();

            Container = containerBuilder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            var initTask = Task.Run(async () => await InitModules(status, Container));
            return initTask;
        }

        private async Task InitModules(UnderConstructionInfo status, IContainer container)
        {
            status.Run();

            try
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
                    foreach (var module in initModules)
                    {
                        status.ClearMessage();
                        await module.Init(serviceLocatorLocal, status);
                    }
                }

                status.Finish();
            }
            catch (InitializationException ie)  when(ie.Subsystem == Subsystem.Database)
            {
                status.Error(Modules.ErrorDuringRunningMigrations);
                container.Resolve<ILogger>().Fatal("Exception during running migrations", 
                    ie.WithFatalType(FatalExceptionType.HqErrorDuringRunningMigrations));
            }
            catch(Exception e)
            {
                status.Error(Modules.ErrorDuringSiteInitialization);
                container.Resolve<ILogger>().Fatal("Exception during site initialization",
                    e.WithFatalType(FatalExceptionType.HqErrorDuringSiteInitialization));
            }
        }
    }
}
