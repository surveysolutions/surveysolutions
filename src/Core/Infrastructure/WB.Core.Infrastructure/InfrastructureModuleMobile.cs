﻿using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.WriteSide;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IEventTypeResolver, EventTypeResolver>();
            registry.Bind<IClock, DateTimeBasedClock>();

            registry.BindAsSingleton<IAggregateRootRepository, AggregateRootRepositoryWithCache>();
            registry.BindAsSingleton<IWriteSideCleanerRegistry, WriteSideCleanerRegistry>();
            registry.BindAsSingleton<ICommandService, SequentialCommandService>();
            registry.BindAsSingleton<ILiteEventRegistry, LiteEventRegistry>();
            registry.Bind<ILiteEventBus, LiteEventBus>();
            registry.BindAsSingletonWithConstructorArgument<ISnapshottingPolicy, SimpleSnapshottingPolicy>("snapshotIntervalInEvents", 1);
            registry.Bind<IAggregateRootCreationStrategy, SimpleAggregateRootCreationStrategy>();
            registry.Bind<IAggregateSupportsSnapshotValidator,AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
        }
    }
}