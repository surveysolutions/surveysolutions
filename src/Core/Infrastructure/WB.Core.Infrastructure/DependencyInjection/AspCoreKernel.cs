using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native;

namespace WB.Core.Infrastructure.DependencyInjection
{
    public class AspCoreKernel
    {
        private readonly IServiceCollection services;
        private readonly IDependencyRegistry dependencyRegistry;

        public AspCoreKernel(IServiceCollection services)
        {
            this.services = services;
            this.dependencyRegistry = new DependencyRegistry(services);

            this.services.AddTransient<IServiceLocator, DotNetCoreServiceLocatorAdapter>();
        }

        protected readonly List<IAppModule> initModules = new List<IAppModule>();

        public virtual void Load(params IAppModule[] modules)
        {
            modules.ForEach(module => module.Load(dependencyRegistry));
            initModules.AddRange(modules);
        }

        public Task InitAsync(IServiceProvider serviceProvider)
        {
            var initTask = Task.Run(async () => await InitModules(serviceProvider));
            return initTask;
        }

        private async Task InitModules(IServiceProvider serviceProvider)
        {
            var status = serviceProvider.GetService<UnderConstructionInfo>();
            status?.Run();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var serviceLocatorLocal = scope.ServiceProvider.GetService<IServiceLocator>();
                    foreach (var module in initModules)
                    {
                        status?.ClearMessage();
                        await module.InitAsync(serviceLocatorLocal, status);
                    }
                }

                status?.Finish();
            }
            catch (InitializationException ie)  when(ie.Subsystem == Subsystem.Database)
            {
                status?.Error(Core.Infrastructure.Resources.Modules.ErrorDuringRunningMigrations, ie);
                serviceProvider.GetService<ILogger<AspCoreKernel>>().LogError(ie, "Exception during running migrations");
            }
            catch(Exception e)
            {
                status?.Error(Core.Infrastructure.Resources.Modules.ErrorDuringSiteInitialization, e);
                serviceProvider.GetService<ILogger<AspCoreKernel>>().LogError(e, "Exception during site initialization");
            }
        }
    }
}
