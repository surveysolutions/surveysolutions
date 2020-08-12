using System.Threading.Tasks;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IAppModule, IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IClock, SystemClock>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.BindInPerLifetimeScope<ICommandService, CommandService>();
            registry.Bind<ICommandExecutor, CommandExecutor>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
            registry.Bind<IAggregateRootCache, AggregateRootCache>();
            registry.Bind<IAggregateRootPrototypeService, AggregateRootPrototypeService>();
            registry.Bind<IAggregateRootPrototypePromoterService, AggregateRootPrototypePromoterService>();
            registry.BindAsSingleton<IDenormalizerRegistry, DenormalizerRegistry>();
            registry.Bind<IInScopeExecutor, NoScopeInScopeExecutor>();
        }

        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IClock, SystemClock>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.BindAsScoped<ICommandService, CommandService>();
            registry.Bind<IAggregateRootCache, AggregateRootCache>();
            registry.Bind<ICommandExecutor, CommandExecutor>();
            registry.Bind<IInScopeExecutor, NoScopeInScopeExecutor>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;
    }
}
