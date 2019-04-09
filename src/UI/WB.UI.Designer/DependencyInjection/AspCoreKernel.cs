using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Resources;
using WB.Infrastructure.Native;

namespace WB.UI.Designer1.DependencyInjection
{
    public class AspCoreKernel
    {
        private readonly IServiceCollection services;
        private readonly IDependencyRegistry dependencyRegistry;

        public AspCoreKernel(IServiceCollection services)
        {
            this.services = services;
            this.dependencyRegistry = new DependencyRegistry(services);
        }

        protected readonly List<IAppModule> initModules = new List<IAppModule>();

        public virtual void Load(params IAppModule[] modules)
        {
            modules.ForEach(module => module.Load(dependencyRegistry));
            initModules.AddRange(modules);
        }

        
        public Task InitAsync(IServiceProvider serviceProvider)
        {
            var status = new UnderConstructionInfo();
            this.services.AddSingleton(typeof(UnderConstructionInfo), sp => status);

            var initTask = Task.Run(async () => await InitModules(status, serviceProvider));
            return initTask;
        }

        private async Task InitModules(UnderConstructionInfo status, IServiceProvider serviceProvider)
        {
            status.Run();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var serviceLocatorLocal = scope.ServiceProvider.GetService<IServiceLocator>();
                    foreach (var module in initModules)
                    {
                        status.ClearMessage();
                        await module.InitAsync(serviceLocatorLocal, status);
                    }
                }

                status.Finish();
            }
            catch (InitializationException ie)  when(ie.Subsystem == Subsystem.Database)
            {
                status.Error(Modules.ErrorDuringRunningMigrations);
                serviceProvider.GetService<ILogger>().Fatal("Exception during running migrations", ie);
            }
            catch(Exception e)
            {
                status.Error(Modules.ErrorDuringSiteInitialization);
                serviceProvider.GetService<ILogger>().Fatal("Exception during site initialization", e);
            }
        }
    }
}
