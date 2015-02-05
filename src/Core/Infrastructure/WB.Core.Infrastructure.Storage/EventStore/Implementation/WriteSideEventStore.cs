﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Utils;
using Newtonsoft.Json.Converters;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    internal class WriteSideEventStore : IStreamableEventStore, IDisposable
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        private static readonly Encoding Encoding = Encoding.UTF8;
        internal static readonly string EventsCategory = "WB";
        internal static readonly string EventsPrefix = EventsCategory + "-";
        private readonly IEventStoreConnection connection;
        bool disposed;
        private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(30);

        internal static readonly string AllEventsStream = "$ce-" + EventsCategory;

        public WriteSideEventStore(IEventStoreConnectionProvider connectionProvider)
        {
            this.connection = connectionProvider.Open();

            using (var cancellationTokenSource = new CancellationTokenSource()) {
                cancellationTokenSource.CancelAfter(this.defaultTimeout);

                this.connection.ConnectAsync().WaitAndUnwrapException(cancellationTokenSource.Token);
            }
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            int normalMin = minVersion > 0 ? (int)Math.Max(0, minVersion - 1) : 0;
            int normalMax = (int)Math.Min(int.MaxValue, maxVersion - 1);
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
                currentSlice = this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(EventsPrefix + id.FormatGuid(), nextSliceStart, batchSize, false));

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
                currentSlice = this.RunWithDefaultTimeout(connection.ReadStreamEventsForwardAsync(AllEventsStream, nextPosition, bulkSize, true));
                nextPosition = currentSlice.NextEventNumber;

                yield return currentSlice.Events.Select(this.ToCommittedEvent).ToArray();
            } while (!currentSlice.IsEndOfStream);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            int expectedStreamVersion = (int) (eventStream.InitialVersion - 1);
            using (EventStoreTransaction transaction = this.RunWithDefaultTimeout(this.connection.StartTransactionAsync(EventsPrefix + eventStream.SourceId.FormatGuid(), expectedStreamVersion)))
            {
                using (var transactionTimeout = new CancellationTokenSource()) 
                {
                    transactionTimeout.CancelAfter(this.defaultTimeout);
                    transaction.WriteAsync(eventStream.Select(BuildEventData)).WaitAndUnwrapException(transactionTimeout.Token);
                }

                using (var commitTimeout = new CancellationTokenSource()) 
                {
                    commitTimeout.CancelAfter(this.defaultTimeout);
                    transaction.CommitAsync().WaitAndUnwrapException(commitTimeout.Token);
                }
            }
        }

        public int CountOfAllEvents()
        {
            StreamEventsSlice slice = this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(AllEventsStream, 0, 1, false));
            return slice.LastEventNumber + 1;
        }

        public long GetLastEventSequence(Guid id)
        {
            StreamEventsSlice slice = this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(EventsPrefix + id.FormatGuid(), 0, 1, false));
            return slice.LastEventNumber + 1;
        }

        private CommittedEvent ToCommittedEvent(ResolvedEvent resolvedEvent)
        {
            string value = Encoding.GetString(resolvedEvent.Event.Data);
            var meta = Encoding.GetString(resolvedEvent.Event.Metadata);
            try
            {
                var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, jsonSerializerSettings);
                var eventData = JsonConvert.DeserializeObject(value,
                                    NcqrsEnvironment.GetEventDataTypeByName(resolvedEvent.Event.EventType.ToPascalCase()),
                                    jsonSerializerSettings);

                var committedEvent = new CommittedEvent(Guid.NewGuid(),
                    metadata.Origin,
                    resolvedEvent.Event.EventId,
                    metadata.EventSourceId,
                    resolvedEvent.Event.EventNumber + 1,
                    metadata.Timestamp,
                    eventData);
                
                return committedEvent;
            }
            catch (JsonException exception)
            {
                throw new SerializationException(string.Format("Error in deserialization of event: {0} of type: {1}. EventSourceId: {2}",
                    resolvedEvent.Event.EventId, resolvedEvent.Event.EventType, resolvedEvent.Event.EventStreamId), exception);
            }
        }

        internal static EventData BuildEventData(UncommittedEvent @event)
        {
            string eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented, jsonSerializerSettings);
            string metaData = JsonConvert.SerializeObject(new EventMetada
            {
                Timestamp = @event.EventTimeStamp,
                Origin = @event.Origin,
                EventSourceId = @event.EventSourceId
            });

            var eventData = new EventData(@event.EventIdentifier,
                @event.Payload.GetType().Name.ToCamelCase(),
                true,
                Encoding.GetBytes(eventString),
                Encoding.GetBytes(metaData));
            return eventData;
        }

        private TResult RunWithDefaultTimeout<TResult>(Task<TResult> readTask)
        {
            using (var timeoutTokenSource = new CancellationTokenSource())
            {
                timeoutTokenSource.CancelAfter(this.defaultTimeout);
                Task timeoutTask = timeoutTokenSource.Token.AsTask();

                Task firstFinishedTask = AsyncContext.Run(() => Task.WhenAny(timeoutTask, readTask));

                if (firstFinishedTask == timeoutTask)
                    throw new TimeoutException(string.Format("Failed to perform eventstore operation using timeout {0}", this.defaultTimeout));

                return ((Task<TResult>)firstFinishedTask).Result;
            }
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