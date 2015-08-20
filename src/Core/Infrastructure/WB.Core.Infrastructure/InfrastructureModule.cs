using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Implementation.WriteSide;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IWriteSideCleanerRegistry, WriteSideCleanerRegistry>();
            registry.Bind<IAggregateRootRepository, AggregateRootRepository>();
            registry.BindAsSingleton<ICommandService, CommandService>();
        }
    }
}