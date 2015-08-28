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
using EventStore.ClientAPI.SystemData;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Portable;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    class WriteSideEventStore : IStreamableEventStore, IDisposable
    {
        const string CountProjectionName = "AllEventsCount";
        const string EventsPrefix = EventsCategory + "-";
        private readonly IEventTypeResolver eventTypeResolver;
        const string EventsCategory = "WB";
        static readonly Encoding Encoding = Encoding.UTF8;
        static long lastUsedGlobalSequence = -1;

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        readonly IEventStoreConnection connection;
        readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(30);
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
            ConfigureEventStore();
            this.connection = connectionProvider.Open();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(this.defaultTimeout);

                this.connection.ConnectAsync().WaitAndUnwrapException(cancellationTokenSource.Token);
            }
        }

        public void Dispose()
        {
            Dispose(true);
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
            var batchSize = normalMax - normalMin;

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start + normalMin;

            do
            {
                currentSlice =
                    this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(GetStreamName(id), nextSliceStart, batchSize,
                        false));

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream && streamEvents.Count < batchSize);

            var storedEvents = streamEvents.Select(this.ToCommittedEvent).ToList();

            return new CommittedEventStream(id, storedEvents);
        }

        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            var position = Position.Start;
            AllEventsSlice slice;

            do
            {
                slice = this.RunWithDefaultTimeout(this.connection.ReadAllEventsForwardAsync(position, settings.MaxCountToRead, false));

                position = slice.NextPosition;

                foreach (var @event in slice.Events)
                {
                    if (!IsSystemEvent(@event))
                        yield return this.ToCommittedEvent(@event);
                }
            } while (!slice.IsEndOfStream);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                var expectedStreamVersion = eventStream.InitialVersion - 1;
                var stream = GetStreamName(eventStream.SourceId);

                var events = eventStream.Select(BuildEventData).ToList();
                using (var writeTimeout = new CancellationTokenSource())
                {
                    writeTimeout.CancelAfter(this.defaultTimeout);

                    this.connection.AppendToStreamAsync(stream, expectedStreamVersion, events)
                        .WaitAndUnwrapException(writeTimeout.Token);
                }
            }
        }

        public int CountOfAllEvents()
        {
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIP), settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(this.logger), httpEndPoint, defaultTimeout);

            var projectionresult =
                AsyncContext.Run(
                    () => manager.GetStateAsync(CountProjectionName, new UserCredentials(this.settings.Login, this.settings.Password)));
            var value = JsonConvert.DeserializeAnonymousType(projectionresult, new
            {
                count = 0
            });

            return value != null ? value.count : 0;
        }

        public long GetLastEventSequence(Guid id)
        {
            var slice = this.RunWithDefaultTimeout(this.connection.ReadStreamEventsForwardAsync(GetStreamName(id), 0, 1, false));
            return slice.LastEventNumber + 1;
        }

        static bool IsSystemEvent(ResolvedEvent @event)
        {
            return !@event.Event.EventStreamId.StartsWith(EventsPrefix);
        }

        static string GetStreamName(Guid eventSourceId)
        {
            return EventsPrefix + eventSourceId.FormatGuid();
        }

        CommittedEvent ToCommittedEvent(ResolvedEvent resolvedEvent)
        {
            var value = Encoding.GetString(resolvedEvent.Event.Data);
            var meta = Encoding.GetString(resolvedEvent.Event.Metadata);
            try
            {
                var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, JsonSerializerSettings);
                var eventData = JsonConvert.DeserializeObject(value,
                    NcqrsEnvironment.GetEventDataTypeByName(resolvedEvent.Event.EventType.ToPascalCase()),
                    JsonSerializerSettings);

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

        internal EventData BuildEventData(UncommittedEvent @event)
        {
            var eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented, JsonSerializerSettings);
            var globalSequence = this.GetNextSequnce();
            var metaData = JsonConvert.SerializeObject(new EventMetada
            {
                Timestamp = @event.EventTimeStamp,
                Origin = @event.Origin,
                EventSourceId = @event.EventSourceId,
                GlobalSequence = globalSequence
            });
            @event.GlobalSequence = globalSequence;
            var eventData = new EventData(@event.EventIdentifier,
                @event.Payload.GetType().Name.ToCamelCase(),
                true,
                Encoding.GetBytes(eventString),
                Encoding.GetBytes(metaData));
            return eventData;
        }

        TResult RunWithDefaultTimeout<TResult>(Task<TResult> readTask)
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
                    if (!IsSystemEvent(@event))
                    {
                        var meta = Encoding.GetString(@event.Event.Metadata);
                        var metadata = JsonConvert.DeserializeObject<EventMetada>(meta, JsonSerializerSettings);
                        lastUsedGlobalSequence = metadata.GlobalSequence;
                        return;
                    }
                }
            } while (!slice.IsEndOfStream);
        }

        private void ConfigureEventStore()
        {
            if (settings.InitializeProjections)
            {
                var httpEndPoint = new IPEndPoint(IPAddress.Parse(this.settings.ServerIP), this.settings.ServerHttpPort);
                var manager = new ProjectionsManager(new EventStoreLogger(this.logger), httpEndPoint, defaultTimeout);

                var userCredentials = new UserCredentials(settings.Login, settings.Password);

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
            if (event.metadata.EventSourceId.indexOf(""$"") !== 0) {
                state.count += 1;
            }
            return state;
        } 
    })";

                    manager.CreateContinuousAsync(CountProjectionName, ProjectionQuery, userCredentials).WaitAndUnwrapException();
                }
            }
        }
    }
}