using Ncqrs;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IClock, DateTimeBasedClock>();
            
            registry.BindAsSingleton<ICommandService, CommandService>();
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
        }
    }
}