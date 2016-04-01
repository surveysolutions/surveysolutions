using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Portable;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using System.IO;
using Newtonsoft.Json.Bson;

namespace WB.Infrastructure.Native.Storage.EventStore.Implementation
{
    class WriteSideEventStore : IStreamableEventStore, IDisposable
    {
        const string CountProjectionName = "AllEventsCountV1";
        const string EventsPrefix = EventsCategory + "-";
        const string StreamDeletedEventType = "$streamDeleted";
        private readonly IEventTypeResolver eventTypeResolver;
        const string EventsCategory = "WB";
        static readonly Encoding Encoding = Encoding.UTF8;
        static long lastUsedGlobalSequence = -1;

        readonly IEventStoreConnection connection;
        readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(30000);
        readonly ILogger logger;
        readonly EventStoreSettings settings;

        bool disposed;
        static object lockObject = new Object();

        public WriteSideEventStore(IEventStoreConnectionProvider connectionProvider,
            ILogger logger,
            EventStoreSettings settings, IEventTypeResolver eventTypeResolver)
        {
            this.logger = logger;
            this.settings = settings;
            this.eventTypeResolver = eventTypeResolver;
            this.ConfigureEventStore();
            this.connection = connectionProvider.Open();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(this.defaultTimeout);

                this.connection.ConnectAsync().WaitAndUnwrapException(cancellationTokenSource.Token);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            var normalMin = minVersion > 0 ? Math.Max(0, minVersion - 1) : 0;
            var normalMax = Math.Min(int.MaxValue, maxVersion - 1);
            if (minVersion > maxVersion)
            {
                return new CommittedEventStream(id);
            }

            var streamEvents = new List<ResolvedEvent>();
            var batchSize = Math.Min(this.settings.MaxCountToRead, normalMax - normalMin);

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start + normalMin;

            do
            {
                currentSlice =
                    this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(GetStreamName(id), nextSliceStart, batchSize,
                        false));

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            var storedEvents = streamEvents.Select(this.ToCommittedEvent).ToList();

            return new CommittedEventStream(id, storedEvents);
        }

        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            Position position = Position.Start;
            AllEventsSlice slice;

            do
            {
                slice = this.RunWithDefaultTimeout(this.connection.ReadAllEventsForwardAsync(position, this.settings.MaxCountToRead, false));

                position = slice.NextPosition;

                foreach (var @event in slice.Events)
                {
                    if (!IsSystemEventOrFromDeletedStream(@event))
                        yield return this.ToCommittedEvent(@event);
                }
            } while (!slice.IsEndOfStream);
        }

        public long GetEventsCountAfterPosition(EventPosition? position)
        {
            var totalCountOfEvents = this.CountOfAllEvents();
            if (!position.HasValue)
                return totalCountOfEvents;

            var eventSlicesAfter = this.GetEventsAfterPosition(position);

            foreach (var eventSlice in eventSlicesAfter)
            {
                var firstEventInSlice = eventSlice.FirstOrDefault();
                if (firstEventInSlice != null)
                    return totalCountOfEvents - firstEventInSlice.GlobalSequence + 1;
            }

            return 0;
        }

        public IEnumerable<EventSlice> GetEventsAfterPosition(EventPosition? position)
        {
            //if end of stream is empty slice
            AllEventsSlice slice;

            Position eventStorePosition = position.HasValue
                ? new Position(position.Value.CommitPosition, position.Value.PreparePosition)
                : Position.Start;

            var shouldLookForLastHandledEvent = position.HasValue;
            EventPosition? previousSliceEventPosition = null;
            do
            {
                slice = this.RunWithDefaultTimeout(this.connection.ReadAllEventsForwardAsync(eventStorePosition, this.settings.MaxCountToRead, false));

                var eventsInSlice = slice.Events.Where(e => !IsSystemEventOrFromDeletedStream(e)).Select(this.ToCommittedEvent).ToArray();

                if (shouldLookForLastHandledEvent)
                {
                    IEnumerable<CommittedEvent> afterLastSuccessfullyHandledEvent =
                        eventsInSlice.SkipWhile(
                            x =>
                                x.EventSourceId != position.Value.EventSourceIdOfLastEvent &&
                                x.EventSequence != position.Value.SequenceOfLastEvent).ToArray();

                    if (afterLastSuccessfullyHandledEvent.Any())
                    {
                        eventsInSlice = afterLastSuccessfullyHandledEvent.Skip(1).ToArray();
                        shouldLookForLastHandledEvent = false;
                    }
                }

                var lastHandledEvent = eventsInSlice.LastOrDefault();

                if (lastHandledEvent == null)
                {
                    if (slice.IsEndOfStream)
                    {
                        if (previousSliceEventPosition.HasValue)
                            yield return
                                new EventSlice(Enumerable.Empty<CommittedEvent>(), previousSliceEventPosition.Value,
                                    true);
                        yield break;
                    }
                    eventStorePosition = slice.NextPosition;
                    continue;
                }

                previousSliceEventPosition = new EventPosition(eventStorePosition.CommitPosition,
                    eventStorePosition.PreparePosition,
                    lastHandledEvent.EventSourceId, 
                    lastHandledEvent.EventSequence);

                yield return
                    new EventSlice(eventsInSlice, previousSliceEventPosition.Value, slice.IsEndOfStream);


                eventStorePosition = slice.NextPosition;

            } while (!slice.IsEndOfStream);
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                var expectedStreamVersion = eventStream.InitialVersion - 1;
                var stream = GetStreamName(eventStream.SourceId);

                List<Tuple<EventData, CommittedEvent>> dataToStore =
                    eventStream.Select(@event => this.BuildEventData(@event, eventStream.CommitId)).ToList();

                var eventDatas = dataToStore.Select(x => x.Item1);
                var committedEvents = dataToStore.Select(x => x.Item2);

                this.RunWithDefaultTimeout(this.connection.AppendToStreamAsync(stream, expectedStreamVersion, eventDatas));
                return new CommittedEventStream(eventStream.SourceId, committedEvents);
            }

            return new CommittedEventStream(eventStream.SourceId);
        }

        public int CountOfAllEvents()
        {
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(this.settings.ServerIP), this.settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(this.logger), httpEndPoint, this.defaultTimeout);

            var projectionresult =
                AsyncContext.Run(
                    () => manager.GetStateAsync(CountProjectionName, new UserCredentials(this.settings.Login, this.settings.Password)));
            var value = JsonConvert.DeserializeAnonymousType(projectionresult, new
            {
                count = 0
            });

            return value != null ? value.count : 0;
        }

        static bool IsSystemEventOrFromDeletedStream(ResolvedEvent @event)
        {
            return !@event.Event.EventStreamId.StartsWith(EventsPrefix) ||
                   @event.Event.EventType.Equals(StreamDeletedEventType, StringComparison.InvariantCultureIgnoreCase);
        }

        static string GetStreamName(Guid eventSourceId)
        {
            return EventsPrefix + eventSourceId.FormatGuid();
        }

        private CommittedEvent ToCommittedEvent(ResolvedEvent resolvedEvent)
        {
            try
            {
                EventMetada metadata;
                IEvent eventData;

                Type resolvedEventType = this.eventTypeResolver.ResolveType(resolvedEvent.Event.EventType.ToPascalCase());
                var meta = Encoding.GetString(resolvedEvent.Event.Metadata);
                metadata = JsonConvert.DeserializeObject<EventMetada>(meta, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

                if (resolvedEvent.Event.IsJson)
                {
                    eventData = JsonConvert.DeserializeObject(Encoding.GetString(resolvedEvent.Event.Data),
                        resolvedEventType,
                        EventSerializerSettings.BackwardCompatibleJsonSerializerSettings) as IEvent;
                }
                else
                {
                    var dataStream = new MemoryStream(resolvedEvent.Event.Data);
                    dataStream.Seek(0, SeekOrigin.Begin);
                    BsonReader dataReader = new BsonReader(dataStream);

                    JsonSerializer serializer = JsonSerializer.Create(EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
                    eventData = serializer.Deserialize(dataReader, resolvedEventType) as IEvent;
                }

                var committedEvent = new CommittedEvent(Guid.NewGuid(),
                    metadata.Origin,
                    resolvedEvent.Event.EventId,
                    metadata.EventSourceId,
                    resolvedEvent.Event.EventNumber + 1,
                    metadata.Timestamp,
                    metadata.GlobalSequence,
                    eventData);

                return committedEvent;
            }
            catch (JsonException exception)
            {
                throw new SerializationException(string.Format("Error in deserialization of event: {0} of type: {1}. EventSourceId: {2}",
                    resolvedEvent.Event.EventId, resolvedEvent.Event.EventType, resolvedEvent.Event.EventStreamId), exception);
            }
        }

        private Tuple<EventData, CommittedEvent> BuildEventData(UncommittedEvent @event, Guid commitId)
        {
            var globalSequence = this.GetNextSequnce();

            var eventMetada = new EventMetada
            {
                Timestamp = @event.EventTimeStamp,
                Origin = @event.Origin,
                EventSourceId = @event.EventSourceId,
                GlobalSequence = globalSequence
            };


            var metaData = JsonConvert.SerializeObject(eventMetada);
            var eventMetadataBytes = Encoding.GetBytes(metaData);
            
            var eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
            var eventDataBytes = Encoding.GetBytes(eventString);
            
            var eventData = new EventData(@event.EventIdentifier,
                @event.Payload.GetType().Name.ToCamelCase(),
                true,
                eventDataBytes,
                eventMetadataBytes);

            var committedEvent = new CommittedEvent(commitId,
                                          @event.Origin,
                                          @event.EventIdentifier,
                                          @event.EventSourceId,
                                          @event.EventSequence,
                                          @event.EventTimeStamp,
                                          globalSequence,
                                          @event.Payload);

            return Tuple.Create(eventData, committedEvent);
        }

        private TResult RunWithDefaultTimeout<TResult>(Task<TResult> readTask)
        {
            using (var timeoutTokenSource = new CancellationTokenSource())
            {
                timeoutTokenSource.CancelAfter(this.defaultTimeout);
                var timeoutTask = timeoutTokenSource.Token.AsTask();

                var firstFinishedTask = AsyncContext.Run(() => Task.WhenAny(timeoutTask, readTask));

                if (firstFinishedTask == timeoutTask)
                    throw new TimeoutException(string.Format("Failed to perform eventstore operation using timeout {0}", this.defaultTimeout));

                return ((Task<TResult>) firstFinishedTask).Result; // this .Result will not invoke task. it is already complete on this stage
            }
        }

        ~WriteSideEventStore()
        {
            this.Dispose(false);
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

        private long GetNextSequnce()
        {
            if (lastUsedGlobalSequence == -1)
            {
                lock (lockObject)
                {
                    if (lastUsedGlobalSequence == -1)
                    {
                        this.FindLastUsedSequenceInEventStore();
                    }
                }
            }

            Interlocked.Increment(ref lastUsedGlobalSequence);
            return lastUsedGlobalSequence;
        }

        private void FindLastUsedSequenceInEventStore()
        {
            var position = Position.End;
            AllEventsSlice slice;

            do
            {
                slice = this.RunWithDefaultTimeout(this.connection.ReadAllEventsBackwardAsync(position, this.settings.MaxCountToRead, false));

                position = slice.NextPosition;

                foreach (var @event in slice.Events)
                {
                    if (!IsSystemEventOrFromDeletedStream(@event))
                    {
                        var meta = Encoding.GetString(@event.Event.Metadata);
                        var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
                        lastUsedGlobalSequence = metadata.GlobalSequence;
                        return;
                    }
                }
            } while (!slice.IsEndOfStream);

            lastUsedGlobalSequence = 0;
        }

        private void ConfigureEventStore()
        {
            if (this.settings.InitializeProjections)
            {
                var httpEndPoint = new IPEndPoint(IPAddress.Parse(this.settings.ServerIP), this.settings.ServerHttpPort);
                var manager = new ProjectionsManager(new EventStoreLogger(this.logger), httpEndPoint, this.defaultTimeout);

                var userCredentials = new UserCredentials(this.settings.Login, this.settings.Password);

                try
                {
                    AsyncContext.Run(() => manager.GetStatusAsync(CountProjectionName, userCredentials));
                }
                catch (ProjectionCommandFailedException)
                {
                    const string ProjectionQuery =
                        @"fromAll() 
    .when({        
        $init: function () {
            return { count: 0 }; // initial state
        },
        $any: function (state, event) {
            if (event.metadata && event.metadata.EventSourceId && event.metadata.EventSourceId.indexOf(""$"") !== 0) {
                state.count += 1;
            }
            return state;
        } 
    })";

                    manager.CreateContinuousAsync(CountProjectionName, ProjectionQuery, userCredentials)
                        .WaitAndUnwrapException();
                }
                catch (Exception exception)
                {
                    this.logger.Fatal("Error on configuration Event Store", exception);
                    throw;
                }
            }
        }
    }
}