using System;
using System.Collections.Generic;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using ILogger = WB.Core.GenericSubdomains.Utils.Services.ILogger;

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
            this.AddEventStoreProjections();
            this.Kernel.Bind<IEventStoreHealthCheck>().ToMethod(_ => new EventStoreHealthCheck(this.settings)).InSingletonScope();
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IStreamableEventStore>(() => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IEventStore>(() => this.Kernel.Get<IEventStore>());
        }

        private IStreamableEventStore GetEventStore()
        {
            return new WriteSideEventStore(new EventStoreConnectionProvider(this.settings));
        }

        private void AddEventStoreProjections()
        {
            var logger = Kernel.Get<ILogger>();
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIP), settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(logger), httpEndPoint, TimeSpan.FromSeconds(2));

            var userCredentials = new UserCredentials(this.settings.Login, this.settings.Password);
            string projectionStatus = AsyncContext.Run(() => manager.GetStatusAsync("$by_category"));
            var status = JsonConvert.DeserializeAnonymousType(projectionStatus, new { status = "" });
            if (status.status != "Running")
            {
                manager.EnableAsync("$by_category", userCredentials).WaitAndUnwrapException();
            }
        }

    }
}