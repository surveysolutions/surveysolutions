using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresEventStoreModule : IModule, IInitModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IHeadquartersEventStore, PostgresEventStore>();
            registry.Bind<IEventStore, PostgresEventStore>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
