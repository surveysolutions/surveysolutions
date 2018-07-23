using System.Threading.Tasks;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IClock, DateTimeBasedClock>();

            registry.BindAsSingleton<IEventSourcedAggregateRootRepositoryWithCache, EventSourcedAggregateRootRepositoryWithCache>();
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepository, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepositoryCacheCleaner, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.BindAsSingleton<ILiteEventRegistry, LiteEventRegistry>();
            registry.Bind<ILiteEventBus, LiteEventBus>();
            registry.BindAsSingletonWithConstructorArgument<ISnapshottingPolicy, SimpleSnapshottingPolicy>("snapshotIntervalInEvents", 1);
            registry.Bind<IAggregateSupportsSnapshotValidator,AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
