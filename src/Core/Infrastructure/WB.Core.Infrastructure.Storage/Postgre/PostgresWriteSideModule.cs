using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresWriteSideModule : NinjectModule
    {
        private readonly PostgreConnectionSettings eventStoreSettings;

        public PostgresWriteSideModule(PostgreConnectionSettings eventStoreSettings)
        {
            this.eventStoreSettings = eventStoreSettings;
        }

        public override void Load()
        {
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.Kernel.Get<IStreamableEventStore>());
            this.Kernel.Bind<IEventStoreApiService>().To<NullIEventStoreApiService>();
        }

        private IStreamableEventStore GetEventStore()
        {
            return new PostgresEventStore(this.eventStoreSettings, this.Kernel.Get<IEventTypeResolver>());
        }

        class NullIEventStoreApiService : IEventStoreApiService
        {
            public void RunScavenge()
            {
            }
        }
    }
}