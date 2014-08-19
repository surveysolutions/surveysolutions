using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using EventStore.ClientAPI;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Utils;
using Newtonsoft.Json.Converters;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    internal class EventStoreWriteSide : IStreamableEventStore, IDisposable
    {
        private readonly IEventStoreConnection connection;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };
        private readonly Encoding encoding = Encoding.UTF8;
        internal const string EventsCategory = "Event";
        private const string EventsPrefix = EventsCategory + "-";

        internal const string AllEventsStream = "all_events";

        public EventStoreWriteSide(EventStoreConnectionSettings connectionSettings)
        {
            this.connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse(connectionSettings.ServerIP), connectionSettings.ServerTcpPort));
            this.connection.Connect();
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = this.connection.ReadStreamEventsForward(EventsPrefix + id.FormatGuid(), nextSliceStart, 200, false);
                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            var storedEvents = streamEvents.Select(this.ToCommittedEvent).ToList();

            return new CommittedEventStream(id, storedEvents);
        }

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize = 200, int skipEvents = 0)
        {
            StreamEventsSlice currentSlice;
            var nextPosition = skipEvents;
            do
            {
                currentSlice = this.connection.ReadStreamEventsForward(AllEventsStream, nextPosition, bulkSize, false);
                nextPosition = currentSlice.NextEventNumber;

                yield return currentSlice.Events.Select(this.ToCommittedEvent).ToArray();
            } while (!currentSlice.IsEndOfStream);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            StreamEventsSlice currentSlice;
            var nextPosition = 0;
            do
            {
                currentSlice = this.connection.ReadStreamEventsForward(AllEventsStream, nextPosition, 200, false);
                nextPosition = currentSlice.NextEventNumber;
                foreach (var resolvedEvent in currentSlice.Events)
                {
                    yield return this.ToCommittedEvent(resolvedEvent);
                }
            } while (!currentSlice.IsEndOfStream);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            using (var transaction = connection.StartTransaction(EventsPrefix + eventStream.SourceId, ExpectedVersion.Any))
            {
                foreach (var @event in eventStream)
                {
                    var eventData = BuildEventData(@event);

                    int expected = (int) (@event.EventSequence == 1 ? ExpectedVersion.NoStream : @event.EventSequence - 2);
                    this.connection.AppendToStream(EventsPrefix + @event.EventSourceId.FormatGuid(), expected, eventData);
                }

                transaction.Commit();
            }
        }

        public int CountOfAllEvents()
        {
            StreamEventsSlice slice = this.connection.ReadStreamEventsBackward(AllEventsStream, int.MaxValue, 1, false);
            return slice.LastEventNumber;
        }

        public long GetLastEventSequence(Guid id)
        {
            var streamMetadataResult = this.connection.GetStreamMetadata(id.FormatGuid());
            return streamMetadataResult.MetastreamVersion;
        }

        public void Dispose()
        {
            this.connection.Dispose();
        }

        private CommittedEvent ToCommittedEvent(ResolvedEvent resolvedEvent)
        {
            string value = this.encoding.GetString(resolvedEvent.Event.Data);
            var meta = this.encoding.GetString(resolvedEvent.Event.Metadata);
            try
            {
                var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, this.jsonSerializerSettings);
                var eventData = JsonConvert.DeserializeObject(value, 
                                    NcqrsEnvironment.GetEventDataTypeByName(resolvedEvent.Event.EventType), 
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
    }
}