using System.Threading.Tasks;
using Ncqrs.Domain.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.Ncqrs
{
    public class NcqrsModule : IModule, IAppModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IDomainRepository, DomainRepository>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }

        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IDomainRepository, DomainRepository>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
