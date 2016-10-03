using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IClock, DateTimeBasedClock>();
            
            registry.Bind<IEventSourcedAggregateRootRepository, EventSourcedAggregateRootRepository>();
            registry.BindAsSingleton<ICommandService, CommandService>();

            registry.BindAsSingletonWithConstructorArgument<ISnapshottingPolicy, SimpleSnapshottingPolicy>("snapshotIntervalInEvents", 1);

            registry.Bind<IAggregateSupportsSnapshotValidator, AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryCachedSnapshotStore>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
        }
    }
}