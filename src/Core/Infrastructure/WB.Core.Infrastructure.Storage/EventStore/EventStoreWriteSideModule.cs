﻿using System;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;

namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreWriteSideModule : NinjectModule
    {
        private readonly EventStoreConnectionSettings settings;

        public EventStoreWriteSideModule(EventStoreConnectionSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        public override void Load()
        {
            this.Kernel.Bind<IEventStoreHealthCheck>().To<EventStoreHealthCheck>().InSingletonScope();
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IStreamableEventStore>(() => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IEventStore>(() => this.Kernel.Get<IEventStore>());
        }

        private IStreamableEventStore GetEventStore()
        {
            return new WriteSideEventStore(new EventStoreConnectionProvider(this.settings), this.Kernel.Get<ILogger>(), this.settings);
        }
    }
}