using System.Threading.Tasks;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule, IAppModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IClock, DateTimeBasedClock>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.BindInPerLifetimeScope<ICommandService, CommandService>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
        }

        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IClock, DateTimeBasedClock>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.BindAsScoped<ICommandService, CommandService>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
