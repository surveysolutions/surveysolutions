using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using Ncqrs.Eventing.Storage;
using Raven.Client;
using Raven.Client.Document;
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
                        eventStore.Store(stream);

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
            IEnumerable<Type> typesInAssembly = typeof (AggregateRootEvent).Assembly.GetTypes();

            if (settings.SelectedAppName == "Designer")
            {
                typesInAssembly = typesInAssembly.Concat(typeof(QuestionnaireCloned).Assembly.GetTypes())
                                                 .Concat(typeof(UserLoggedIn).Assembly.GetTypes());
            }
            else
            {
                typesInAssembly = typesInAssembly.Concat(typeof (DataCollectionSharedKernelModule).Assembly.GetTypes());
            }
            var q = from t in typesInAssembly
                    where t.IsClass && !t.IsAbstract && t.Namespace != null && (t.Namespace.Contains("Events"))
                    select t;
            q.ToList().ForEach(NcqrsEnvironment.RegisterEventDataType);
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