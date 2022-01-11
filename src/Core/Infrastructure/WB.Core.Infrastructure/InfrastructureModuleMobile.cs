using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IEventSourcedAggregateRootRepositoryWithCache, EventSourcedAggregateRootRepositoryWithCache>();
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepository, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepositoryCacheCleaner, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.Bind<IAggregateRootCache, AggregateRootCache>();
            registry.BindAsSingleton<ICommandsMonitoring, TraceCommandsMonitoring>();
            registry.BindAsSingleton<IDenormalizerRegistry, DenormalizerRegistry>();
            registry.Bind<ICommandExecutor, CommandExecutor>();
            registry.Bind<IAggregateRootPrototypeService, DummyAggregateRootPrototypeService>();
            registry.Bind<IAggregateRootPrototypePromoterService, DummyAggregateRootPrototypePromoterService>();
        }
    }
}
