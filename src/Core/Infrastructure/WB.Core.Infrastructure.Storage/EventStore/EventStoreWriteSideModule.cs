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
            this.AddEventStoreProjections();
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IStreamableEventStore>(() => this.Kernel.Get<IStreamableEventStore>());
            NcqrsEnvironment.SetGetter<IEventStore>(() => this.Kernel.Get<IEventStore>());
        }

        private IStreamableEventStore GetEventStore()
        {
            return new WriteSideEventStore(this.settings);
        }

        private void AddEventStoreProjections()
        {
            var logger = Kernel.Get<GenericSubdomains.Logging.ILogger>();
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIP), settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(logger), httpEndPoint, TimeSpan.FromSeconds(2));

            var userCredentials = new UserCredentials(this.settings.Login, this.settings.Password);
            try
            {
                var status = JsonConvert.DeserializeAnonymousType(manager.GetStatusAsync("$by_category").Result, new { status = "" });
                if (status.status != "Running")
                {
                    manager.EnableAsync("$by_category", userCredentials);
                }
                manager.GetStatusAsync("ToAllEvents", userCredentials).Wait();
            }
            catch (AggregateException)
            {
                string projectionQuery = @"fromCategory('" + WriteSideEventStore.EventsCategory + @"') 
                                                .when({        
                                                    $any: function (s, e) {
                                                        linkTo('" + WriteSideEventStore.AllEventsStream + @"', e)
                                                    }
                                                })";
                manager.CreateContinuousAsync("ToAllEvents", projectionQuery, userCredentials);
            }
        }
    }
}