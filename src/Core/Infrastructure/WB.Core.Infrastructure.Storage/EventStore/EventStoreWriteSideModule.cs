using System;
using System.Collections.Generic;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using Ncqrs;
using Ncqrs.Eventing.Storage;
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
            NcqrsEnvironment.SetGetter<IStreamableEventStore>(this.GetEventStore);
            NcqrsEnvironment.SetGetter<IEventStore>(this.GetEventStore);
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.GetEventStore()).InSingletonScope();
           
        }

        private IStreamableEventStore GetEventStore()
        {
            return new EventStoreWriteSide(this.settings);
        }

        private void AddEventStoreProjections()
        {
            var logger = Kernel.Get<GenericSubdomains.Logging.ILogger>();
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIP), settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(logger), httpEndPoint, TimeSpan.FromSeconds(2));

            try
            {
                string result = manager.GetStatusAsync("ToAllEvents", new UserCredentials(this.settings.Login, this.settings.Password)).Result;
            }
            catch (AggregateException)
            {
                const string ProjectionQuery = @"fromCategory('" + EventStoreWriteSide.EventsCategory + @"') 
    .when({        
        $any: function (s, e) {
            linkTo('" + EventStoreWriteSide.AllEventsStream + @"', e)
        }
    })
  ";
                manager.CreateContinuousAsync("ToAllEvents", ProjectionQuery, new UserCredentials(this.settings.Login, this.settings.Password));
            }
        }
    }
}