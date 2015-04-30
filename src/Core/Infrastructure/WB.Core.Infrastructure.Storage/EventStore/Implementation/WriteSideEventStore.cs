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
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Utils;
using Newtonsoft.Json.Converters;
using ILogger = WB.Core.GenericSubdomains.Utils.Services.ILogger;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    internal class WriteSideEventStore : IStreamableEventStore, IDisposable
    {
        private const string CountProjectionName = "AllEventsCount";
        private readonly ILogger logger;
        private readonly EventStoreSettings settings;
        private static readonly Encoding Encoding = Encoding.UTF8;
        private const string EventsPrefix = EventsCategory + "-";
        private const string EventsCategory = "WB";
        private readonly IEventStoreConnection connection;
        private bool disposed;
        private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(30);

        internal static readonly string AllEventsStream = "$ce-" + EventsCategory;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        public WriteSideEventStore(IEventStoreConnectionProvider connectionProvider, 
            ILogger logger,
            EventStoreSettings settings)
        {
            this.logger = logger;
            this.settings = settings;
            ConfigureEventStore();
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

        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            var position = Position.Start;
            AllEventsSlice slice;

            do
            {
                slice = this.RunWithDefaultTimeout(this.connection.ReadAllEventsForwardAsync(position, settings.MaxCountToRead, resolveLinkTos: false));

                position = slice.NextPosition;

                foreach (var @event in slice.Events)
                {
                    if (!IsSystemEvent(@event))
                        yield return this.ToCommittedEvent(@event);
                }

            } while (!slice.IsEndOfStream);
        }

        private static bool IsSystemEvent(ResolvedEvent @event)
        {
            return !@event.Event.EventStreamId.StartsWith(EventsPrefix);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            int expectedStreamVersion = (int) (eventStream.InitialVersion - 1);

            using (var writeTimeout = new CancellationTokenSource())
            {
                writeTimeout.CancelAfter(this.defaultTimeout);

                var stream = EventsPrefix + eventStream.SourceId.FormatGuid();
                var events = eventStream.Select(BuildEventData);
                this.connection.AppendToStreamAsync(stream, expectedStreamVersion, events)
                               .WaitAndUnwrapException(writeTimeout.Token);
            }
        }

        public int CountOfAllEvents()
        {
            var httpEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIP), settings.ServerHttpPort);
            var manager = new ProjectionsManager(new EventStoreLogger(this.logger), httpEndPoint, defaultTimeout);

            string projectionresult = AsyncContext.Run(() => manager.GetStateAsync(CountProjectionName, new UserCredentials(this.settings.Login, this.settings.Password)));
            var value = JsonConvert.DeserializeAnonymousType(projectionresult, new
            {
                count = 0
            });

            return value != null ? value.count : 0;
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
            string eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented, JsonSerializerSettings);
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

                return ((Task<TResult>)firstFinishedTask).Result; // this .Result will not invoke task. it is already complete on this stage
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