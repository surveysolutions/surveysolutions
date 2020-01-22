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
        public AutofacKernel() : this(new ContainerBuilder())
        {
        }

        public AutofacKernel(ContainerBuilder containerBuilder)
        {
            this.containerBuilder = containerBuilder;

            containerBuilder.RegisterBuildCallback(container => Container = container);

            this.containerBuilder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>().InstancePerLifetimeScope();

            var status = new UnderConstructionInfo();
            this.containerBuilder.Register((ctx, p) => status).SingleInstance();
        }

        protected readonly ContainerBuilder containerBuilder;

        protected readonly List<IInitModule> initModules = new List<IInitModule>();

        public ContainerBuilder ContainerBuilder => containerBuilder;

        public ILifetimeScope Container { get; set; }

        public virtual void Load<T>(params IModule<T>[] modules) where T: IIocRegistry
        {
            var autofacModules = modules.Select(module =>
            {
                switch (module)
                {
                    case IModule iModule:
                        return iModule.AsAutofac();
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


        public Task InitAsync(bool restartOnInitializationError)
        {
            if (Container == null)
                throw new ArgumentException("Container should be build before init");
            
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            var initTask = Task.Run(() => InitModules(Container.Resolve<UnderConstructionInfo>(), Container, restartOnInitializationError));
            return initTask;
        }

        private async Task InitModules(UnderConstructionInfo status, ILifetimeScope container,
            bool restartOnInitializationError)
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
                status.Error(Modules.ErrorDuringRunningMigrations, ie);
                container.Resolve<ILogger>().Fatal("Exception during running migrations", 
                    ie.WithFatalType(FatalExceptionType.HqErrorDuringRunningMigrations));
                ScheduleAppReboot();
            }
            catch(Exception e)
            {
                status.Error(Modules.ErrorDuringSiteInitialization, e);
                
                container.Resolve<ILogger>().Fatal("Exception during site initialization",
                    e.WithFatalType(FatalExceptionType.HqErrorDuringSiteInitialization));
                ScheduleAppReboot();
            }

            void ScheduleAppReboot()
            {
                if (restartOnInitializationError && !scheduledAppReboot)
                {
                    container.Resolve<ILogger>().Error("Scheduled application pool reboot in 10 seconds");

                    scheduledAppReboot = true;
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        AppDomain.Unload(AppDomain.CurrentDomain);
                    });
                }
            }
        }

        private static bool scheduledAppReboot = false;
    }
}
