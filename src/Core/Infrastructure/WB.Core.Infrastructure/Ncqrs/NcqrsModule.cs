using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Domain.Storage;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.Ncqrs
{
    public class NcqrsModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IDomainRepository, DomainRepository>();
            NcqrsEnvironment.SetGetter<IDomainRepository>(() => ServiceLocator.Current.GetInstance<IDomainRepository>());
        }
    }
}