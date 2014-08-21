using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Main.Core.Events;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Raven.Client;
using Raven.Client.Document;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tools.EventsMigrator
{
    public class Executor
    {
        private string status;
        private int totalEvents;
        private int processed;
        private TimeSpan elapsed;

        public void Process(IShell settings)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var ravenStore = GetRavenStore(settings);
            var eventStore = GetEventStoreStore(settings);

            this.status = "Counting number of events to process";
            this.totalEvents = ravenStore.CountOfAllEvents();

            this.processed = settings.SkipEvents;

            RegisterEvents(settings);

            Stopwatch watch = Stopwatch.StartNew();

            this.status = "Getting events from event store";

            using (var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse(settings.EventStoreIP), settings.EventStoreTcpPort)))
            {
                connection.ConnectAsync().Wait();

                foreach (CommittedEvent[] @event in ravenStore.GetAllEvents(50, settings.SkipEvents))
                {
                    foreach (var committedEvent in @event)
                    {
                        var stream = new UncommittedEventStream(Guid.NewGuid(), committedEvent.Origin);
                        stream.Append(new UncommittedEvent(committedEvent.EventIdentifier,
                            committedEvent.EventSourceId,
                            committedEvent.EventSequence,
                            0,
                            committedEvent.EventTimeStamp,
                            committedEvent.Payload,
                            committedEvent.EventVersion));
                        
                        
                        eventStore.SaveStream(stream, connection);

                        Interlocked.Increment(ref processed);
                        this.elapsed = watch.Elapsed;
                    }
                }
            }

            this.status = "Done writing events";
            watch.Stop();
        }

        private static EventStoreWriteSide GetEventStoreStore(IShell settings)
        {
            var instance = new EventStoreWriteSide(new EventStoreConnectionSettings
            {
                ServerIP = settings.EventStoreIP,
                ServerHttpPort = settings.EventStoreHttpPort,
                ServerTcpPort = settings.EventStoreTcpPort,
                Login = settings.EventStoreLogin,
                Password = settings.EventStorePassword
            });
            return instance;
        }

        private static RavenDBEventStore GetRavenStore(IShell settings)
        {
            var ravenConnectionSettings = new RavenConnectionSettings(
                   settings.ServerAddress,
                   isEmbedded: false,
                   eventsDatabase: settings.RavenDatabaseName);

            var instance = new DocumentStoreProvider(ravenConnectionSettings);
            DocumentStore separateInstanceForEventStore = instance.CreateSeparateInstanceForEventStore();
            var store = new RavenDBEventStore(separateInstanceForEventStore, 1000, useStreamingForAllEvents: false);
            return store;
        }

        private static void RegisterEvents(IShell settings)
        {
            var assemblies = new List<Assembly> { typeof (AggregateRootEvent).Assembly };

            if (settings.SelectedAppName == "Designer")
            {
                assemblies.Add(typeof(DesignerBoundedContextModule).Assembly);
                assemblies.Add(typeof(UserLoggedIn).Assembly);
            }
            else
            {
                assemblies.Add(typeof (DataCollectionSharedKernelModule).Assembly);
            }

            var types = GetAllEventTypes(assemblies).ToList();
            types.ForEach(NcqrsEnvironment.RegisterEventDataType);
        }

        private static IEnumerable<Type> GetAllEventTypes(IEnumerable<Assembly> assemblies)
        {
            return (from assembly in assemblies 
                   from type in assembly.GetTypes() 
                   where IsEventHandler(type) 
                        from handledEventType in GetHandledEventTypes(type) 
                   select handledEventType).Distinct();
        }

        private static IEnumerable<Type> GetHandledEventTypes(Type type)
        {
            foreach (var handlerInterfaceType in type.GetInterfaces().Where(IsIEventHandlerInterface))
            {
                var eventDataType = handlerInterfaceType.GetGenericArguments().First();
                yield return eventDataType;
            }
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var isIEventHandlerInterface = type.IsInterface &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEventHandler<>);
            return isIEventHandlerInterface;
        }

        private static bool IsEventHandler(Type type)
        {
            return type.GetInterfaces().Any(x =>
                                      x.IsGenericType &&
                                      x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }


        public ExecutionStatus GetStatus()
        {
            return new ExecutionStatus
            {
                Status = this.status,
                Total = this.totalEvents,
                Elapsed = this.elapsed,
                Processed = this.processed,
            };
        }
    }

    public class ExecutionStatus
    {
        private TimeSpan elapsed;
        public string Status { get; set; }

        public int Processed { get; set; }

        public int Total { get; set; }

        public TimeSpan Elapsed
        {
            get { return elapsed; }
            set { elapsed = value; }
        }
    }
}