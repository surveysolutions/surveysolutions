using System.Threading.Tasks;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EventSourcedInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<ISnapshottingPolicy, SimpleSnapshottingPolicy>("snapshotIntervalInEvents", 1);

            registry.Bind<IAggregateSupportsSnapshotValidator, AggregateSupportsSnapshotValidator>();
            registry.Bind<IAggregateSnapshotter, DefaultAggregateSnapshotter>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryCachedSnapshotStore>();
        }

        public Task Init(IServiceLocator serviceLocator)
        {
            return Task.CompletedTask;
        }
    }
}
