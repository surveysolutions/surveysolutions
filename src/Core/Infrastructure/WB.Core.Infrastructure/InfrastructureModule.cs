using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Implementation.WriteSide;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IEventTypeResolver, EventTypeResolver>();
            registry.Bind<IClock, DateTimeBasedClock>();

            registry.BindAsSingleton<IWriteSideCleanerRegistry, WriteSideCleanerRegistry>();
            registry.Bind<IAggregateRootRepository, AggregateRootRepository>();
            registry.BindAsSingleton<ICommandService, CommandService>();

            registry.BindAsSingletonWithConstructorArgument<ISnapshottingPolicy, SimpleSnapshottingPolicy>("snapshotIntervalInEvents", 1);
            registry.Bind<IAggregateRootCreationStrategy, SimpleAggregateRootCreationStrategy>();

            registry.Bind<IAggregateSupportsSnapshotValidator, AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
            registry.Bind<ISnapshotStore, InMemoryCachedSnapshotStore>();
        }
    }
}