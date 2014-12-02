using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Implementation.Snapshots;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Snapshots;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAggregateRootRepository, AggregateRootRepository>();
            registry.Bind<ISnapshooter, Snapshooter>();
            registry.Bind<ICommandService, CommandService>();
        }
    }
}