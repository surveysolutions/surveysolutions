using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Polly;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

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

        public AutofacKernel(ContainerBuilder containerBuilder, Action<IContainer> onBuildAction = null)
        {
            this.containerBuilder = containerBuilder;

            containerBuilder.RegisterBuildCallback(container =>
            {
                Container = container;
                onBuildAction?.Invoke(container);
            });

            this.containerBuilder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>()
                .InstancePerLifetimeScope();

            var status = new UnderConstructionInfo();
            this.containerBuilder.Register((ctx, p) => status).SingleInstance();
        }

        private readonly ContainerBuilder containerBuilder;

        private readonly List<IInitModule> initModules = new List<IInitModule>();

        public ContainerBuilder ContainerBuilder => containerBuilder;

        private ILifetimeScope Container { get; set; }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public virtual void Load<T>(params IModule<T>[] modules) where T : IIocRegistry
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
            
            initModules.AddRange(modules.OfType<IInitModule>());
        }

        public Task InitAsync(bool restartOnInitializationError)
        {
            if (Container == null)
                throw new ArgumentException("Container should be build before init");

            if (restartOnInitializationError && !Container.IsRegistered<IApplicationRestarter>())
                throw new ArgumentException(
                    "For restart application need implement and register IApplicationRestarter");

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(Container));

            var initTask = Task.Run(async () =>
            {
                try
                {
                    await InitModules(Container);
                }
                catch (Exception e)
                {
                    if (Container != null)
                    {
                        var log = Container.Resolve<ILogger<AutofacKernel>>();
                        log.LogError(e, "Unable to start modules init");
                    }
                    else
                    {
                        Console.Error.WriteLine("Unable to start modules init: " + e.ToString());
                    }
                }
            });
            return initTask;
        }

        private async Task InitModules(ILifetimeScope container)
        {
            var status = container.Resolve<UnderConstructionInfo>();
            var log = container.Resolve<ILogger<AutofacKernel>>();
            log.LogDebug("Initiating modules");

            status.Run();
            
            using var scope = container.BeginLifetimeScope();

            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var global = Stopwatch.StartNew();
            
            foreach (var module in initModules)
            {
                var init = Stopwatch.StartNew();
                status.ClearMessage();

                try
                {
                    await Policy.Handle<InitializationException>(e => e.IsTransient)
                        .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(Math.Max(i, 5)),
                            (exception, span) =>
                                status.Run($"Retry in {span.TotalSeconds:0}s due to error: {exception.Message}"))
                        .ExecuteAsync(async () =>
                        {
                            await module.Init(serviceLocatorLocal, status);
                            status.ClearMessage();
                        });
                }
                catch (InitializationException ie) when (ie.Subsystem == Subsystem.Database)
                {
                    status.Error(Modules.ErrorDuringRunningMigrations, ie);
                    log.LogCritical(ie.WithFatalType(FatalExceptionType.HqErrorDuringRunningMigrations),
                        "Exception during running migrations. Connection string: " + ie.Data["ConnectionString"]);
                }
                catch (DependencyResolutionException dre) when (dre.InnerException != null)
                {
                    status.Error(Modules.ErrorDuringSiteInitialization, dre.InnerException);

                    log.LogCritical(dre.InnerException.WithFatalType(FatalExceptionType.HqErrorDuringSiteInitialization),
                        "Exception during site initialization");
                }
                catch (Exception e)
                {
                    status.Error(Modules.ErrorDuringSiteInitialization, e);

                    log.LogCritical(e.WithFatalType(FatalExceptionType.HqErrorDuringSiteInitialization), 
                        "Exception during site initialization");
                }
                
                init.Stop();
                log.LogTrace("[{module}] Initialized in {seconds:0.00}s", module.GetType().Name, init.Elapsed.TotalSeconds);
            }

            log.LogDebug("All modules are initialized in {seconds:0.00}s", global.Elapsed.TotalSeconds);
            
            if (status.Status != UnderConstructionStatus.Error)
            {
                status.Finish();
            }
        }
    }
}
