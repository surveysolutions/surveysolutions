using System.Threading.Tasks;
using Ncqrs;
using Ncqrs.Domain.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.Ncqrs
{
    public class NcqrsModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IDomainRepository, DomainRepository>();
            registry.Bind<IClock, DateTimeBasedClock>();
        }
    }
}
