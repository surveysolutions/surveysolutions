using System;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.EventStore.Implementation;

namespace WB.Infrastructure.Native.Storage.EventStore
{
    public class EventStoreWriteSideModule : NinjectModule
    {
        private readonly EventStoreSettings settings;

        public EventStoreWriteSideModule(EventStoreSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        public override void Load()
        {
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStoreApiService>().ToMethod(_ => this.GetEventStoreApiService()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.Kernel.Get<IStreamableEventStore>());
        }

        private IEventStoreApiService GetEventStoreApiService()
        {
            return new EventStoreApiService(this.settings);
        }

        private IStreamableEventStore GetEventStore()
        {
            return new WriteSideEventStore(new EventStoreConnectionProvider(this.settings), this.Kernel.Get<ILogger>(), this.settings, this.Kernel.Get<IEventTypeResolver>());
        }
    }
}