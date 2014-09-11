using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Utils;
using Newtonsoft.Json.Converters;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    internal class WriteSideEventStore : IStreamableEventStore, IDisposable
    {
        private readonly EventStoreConnectionSettings connectionSettings;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        private readonly Encoding encoding = Encoding.UTF8;
        private readonly UserCredentials credentials;
        internal static readonly string EventsCategory = "WB";
        private static readonly string EventsPrefix = EventsCategory + "-";
        private IEventStoreConnection connection;
        bool disposed;

        internal const string AllEventsStream = "$ce-WB";

        public WriteSideEventStore(EventStoreConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
            this.credentials = new UserCredentials(this.connectionSettings.Login, this.connectionSettings.Password);
            this.connection = this.GetConnection();
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            int normalMin = minVersion > 0 ? (int) Math.Max(0, minVersion - 1) : 0;
            int normalMax = (int) Math.Min(int.MaxValue, maxVersion - 1);
            if (minVersion > maxVersion)
            {
                return new CommittedEventStream(id);
            }

            var streamEvents = new List<ResolvedEvent>();
            int batchSize = normalMax - normalMin;

            StreamEventsSlice currentSlice;
            int nextSliceStart = StreamPosition.Start + normalMin;

            do
            {
                currentSlice = connection.ReadStreamEventsForwardAsync(EventsPrefix + id.FormatGuid(), nextSliceStart, batchSize, false).Result;
                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream && streamEvents.Count < batchSize);

            var storedEvents = streamEvents.Select(this.ToCommittedEvent).ToList();

            return new CommittedEventStream(id, storedEvents);
        }

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize = 200, int skipEvents = 0)
        {
            var nextPosition = skipEvents;
            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = connection.ReadStreamEventsForwardAsync(AllEventsStream, nextPosition, bulkSize, true, this.credentials).Result;
                nextPosition = currentSlice.NextEventNumber;

                yield return currentSlice.Events.Select(this.ToCommittedEvent).ToArray();
            } while (!currentSlice.IsEndOfStream);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            var nextPosition = 0;
            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = connection.ReadStreamEventsForwardAsync(AllEventsStream, nextPosition, 200, false, this.credentials).Result;
                nextPosition = currentSlice.NextEventNumber;
                foreach (var resolvedEvent in currentSlice.Events)
                {
                    yield return this.ToCommittedEvent(resolvedEvent);
                }
            } while (!currentSlice.IsEndOfStream);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            using (var transaction = connection.StartTransactionAsync(EventsPrefix + eventStream.SourceId, ExpectedVersion.Any, this.credentials).Result)
            {
                this.SaveStream(eventStream, connection);

                transaction.Commit();
            }
        }

        internal void SaveStream(UncommittedEventStream eventStream, IEventStoreConnection connection)
        {
            foreach (var @event in eventStream)
            {
                var eventData = this.BuildEventData(@event);

                int expected = (int)(@event.EventSequence == 1 ? ExpectedVersion.Any : @event.EventSequence - 2);
                connection.AppendToStreamAsync(EventsPrefix + @event.EventSourceId.FormatGuid(), expected, this.credentials, eventData)
                    .Wait(TimeSpan.FromSeconds(2));
            }
        }

        public int CountOfAllEvents()
        {
            StreamEventsSlice slice = this.connection.ReadStreamEventsForwardAsync(AllEventsStream, 0, 1, false, this.credentials).Result;
            return slice.LastEventNumber + 1;
        }

        public long GetLastEventSequence(Guid id)
        {
            StreamEventsSlice slice = this.connection.ReadStreamEventsForwardAsync(EventsPrefix + id.FormatGuid(), 0, 1, false, this.credentials).Result;
            return slice.LastEventNumber + 1;
        }

        private CommittedEvent ToCommittedEvent(ResolvedEvent resolvedEvent)
        {
            string value = this.encoding.GetString(resolvedEvent.Event.Data);
            var meta = this.encoding.GetString(resolvedEvent.Event.Metadata);
            try
            {
                var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, this.jsonSerializerSettings);
                var eventData = JsonConvert.DeserializeObject(value,
                                    NcqrsEnvironment.GetEventDataTypeByName(resolvedEvent.Event.EventType.ToPascalCase()),
                                    this.jsonSerializerSettings);

                var committedEvent = new CommittedEvent(Guid.NewGuid(),
                    metadata.Origin,
                    resolvedEvent.Event.EventId,
                    metadata.EventSourceId,
                    resolvedEvent.Event.EventNumber + 1,
                    metadata.Timestamp,
                    eventData,
                    new Version());
                return committedEvent;
            }
            catch (JsonException exception)
            {
                throw new SerializationException(string.Format("Error in deserialization of event: {0}, Data: {1}, Meta: {2}",
                    resolvedEvent.Event.EventId, value, meta), exception);
            }
        }

        private EventData BuildEventData(UncommittedEvent @event)
        {

            string eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented, this.jsonSerializerSettings);
            string metaData = JsonConvert.SerializeObject(new EventMetada
            {
                Timestamp = @event.EventTimeStamp,
                Origin = @event.Origin,
                EventSourceId = @event.EventSourceId
            });

            var eventData = new EventData(@event.EventIdentifier,
                @event.Payload.GetType().Name.ToCamelCase(),
                true,
                this.encoding.GetBytes(eventString),
                this.encoding.GetBytes(metaData));
            return eventData;
        }

        private IEventStoreConnection GetConnection()
        {
            if (this.connection == null)
            {
                var eventStoreConnection =
                    EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse(this.connectionSettings.ServerIP),
                        this.connectionSettings.ServerTcpPort));
                eventStoreConnection.ConnectAsync().Wait(TimeSpan.FromSeconds(5));
                return eventStoreConnection;
            }
            return this.connection;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WriteSideEventStore()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.connection.Close();
            }

            this.disposed = true;
        }

    }
}