using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Singleton<IAggregateRootRepository, AggregateRootRepositoryWithCache>();
            registry.Bind<ICommandService, CommandService>();
            registry.Singleton<ILiteEventRegistry, LiteEventRegistry>();
            registry.Bind<ILiteEventBus, LiteEventBus>();
            registry.Bind<ISnapshottingPolicy, NoSnapshottingPolicy>();
            registry.Bind<IAggregateRootCreationStrategy, SimpleAggregateRootCreationStrategy>();
            registry.Bind<IAggregateSupportsSnapshotValidator,AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
            registry.Bind<IDomainRepository, DomainRepository>();
        }
    }
}