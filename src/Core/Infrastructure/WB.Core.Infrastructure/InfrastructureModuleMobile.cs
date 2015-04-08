using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAggregateRootRepository, InMemoryAggregateRootRepository>();
            registry.Bind<ICommandService, CommandService>();
        }
    }
}