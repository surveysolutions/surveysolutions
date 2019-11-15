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

            this.containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            this.containerBuilder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>().InstancePerLifetimeScope();

            var status = new UnderConstructionInfo();
            this.containerBuilder.Register((ctx, p) => status).SingleInstance();
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


        public Task InitAsync(bool restartOnInitiazationError)
        {
            Container = containerBuilder.Build();
            return InitModules(restartOnInitiazationError);
        }

        public Task InitCoreAsync(IContainer container, bool restartOnInitiazationError)
        {
            Container = container;

            return InitModules(restartOnInitiazationError);
        }

        private Task InitModules(bool restartOnInitiazationError)
        {
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            var initTask = Task.Run(async () =>
                await InitModules(Container.Resolve<UnderConstructionInfo>(), Container, restartOnInitiazationError));
            return initTask;
        }

        private async Task InitModules(UnderConstructionInfo status, IContainer container,
            bool restartOnInitiazationError)
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
                if (restartOnInitiazationError && !scheduledAppReboot)
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
