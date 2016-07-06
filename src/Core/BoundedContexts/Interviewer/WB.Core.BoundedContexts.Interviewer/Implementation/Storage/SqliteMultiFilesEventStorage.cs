using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteMultiFilesEventStorage : IInterviewerEventStorage
    {
        private SQLiteConnectionWithLock eventStoreInSingleFile;
        internal readonly Dictionary<Guid, SQLiteConnectionWithLock> connectionByEventSource = new Dictionary<Guid, SQLiteConnectionWithLock>();
        private readonly SqliteSettings settings;
        private readonly IEnumeratorSettings enumeratorSettings;

        private readonly ISQLitePlatform sqLitePlatform;
        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IEventTypeResolver eventTypeResolver;
        private ITraceListener traceListener;

        private string connectionStringToEventStoreInSingleFile;
        private static readonly Object lockObject = new Object();
        static readonly Encoding TextEncoding = Encoding.UTF8;
        private readonly EventSerializer eventSerializer;
        private readonly BackwardCompatibleEventSerializer backwardCompatibleEventSerializer;

        public SqliteMultiFilesEventStorage(ISQLitePlatform sqLitePlatform,
            ILogger logger,
            ITraceListener traceListener,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings,
            IFileSystemAccessor fileSystemAccessor,
            IEventTypeResolver eventTypeResolver)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.eventTypeResolver = eventTypeResolver;
            this.settings = settings;
            this.enumeratorSettings = enumeratorSettings;
            this.sqLitePlatform = sqLitePlatform;
            this.logger = logger;
            this.traceListener = traceListener;
            this.eventSerializer = new EventSerializer(eventTypeResolver);
            this.backwardCompatibleEventSerializer = new BackwardCompatibleEventSerializer();

            this.InitializeEventStoreInSingleFile();
        }

        private SQLiteConnectionWithLock CreateConnection(string connectionString)
        {
            var connection = new SQLiteConnectionWithLock(this.sqLitePlatform,
                new SQLiteConnectionString(connectionString, true,
                    new BlobSerializerDelegate(
                        (obj) => TextEncoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None)),
                        (data, type) => JsonConvert.DeserializeObject(TextEncoding.GetString(data, 0, data.Length), type),
                        (type) => true),
                    openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex))
            {
                //TraceListener = traceListener
            };

            connection.CreateTable<EventView>();
            connection.CreateIndex<EventView>(entity => entity.EventId);

            return connection;
        }

        private string GetEventSourceConnectionString(Guid eventSourceId)
            => this.ToSqliteConnectionString(Path.Combine(this.settings.PathToInterviewsDirectory, $"{eventSourceId.FormatGuid()}.sqlite3"));

        private string ToSqliteConnectionString(string pathToDatabase)
            => this.settings.InMemoryStorage ? $"file:{pathToDatabase}?mode=memory" : pathToDatabase;

        private SQLiteConnectionWithLock GetOrCreateConnection(Guid eventSourceId)
        {
            SQLiteConnectionWithLock connection;

            if (!this.connectionByEventSource.TryGetValue(eventSourceId, out connection))
            {
                lock (lockObject)
                {
                    if (!this.connectionByEventSource.TryGetValue(eventSourceId, out connection))
                    {
                        connection = this.CreateConnection(this.GetEventSourceConnectionString(eventSourceId));
                        this.connectionByEventSource.Add(eventSourceId, connection);
                    }
                }
            }

            return connection;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
            => this.Read(id, minVersion, null, CancellationToken.None);

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEnumerable<CommittedEvent> events = null;

            var eventSourceFilePath = this.GetEventSourceConnectionString(id);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                events = this.Read(this.GetOrCreateConnection(id), id, minVersion, this.eventSerializer, progress, cancellationToken);
            }

            return events ?? this.ReadFromEventStoreInSingleFile(id, minVersion, progress, cancellationToken) ?? new CommittedEvent[0];
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
            => this.StoreToEventStoreInSingleFile(eventStream) ?? this.Store(this.GetOrCreateConnection(eventStream.SourceId), eventStream, this.eventSerializer);

        public void RemoveEventSourceById(Guid interviewId)
        {
            var eventSourceFilePath = this.GetEventSourceConnectionString(interviewId);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                lock (lockObject)
                {
                    if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
                    {
                        SQLiteConnectionWithLock connection;
                        if (this.connectionByEventSource.TryGetValue(interviewId, out connection))
                        {
                            connection.Dispose();
                            this.connectionByEventSource.Remove(interviewId);
                        }

                        this.fileSystemAccessor.DeleteFile(eventSourceFilePath);
                    }
                }
            }
            
            this.RemoveFromEventStoreInSingleFile(interviewId);
        }

        public void Dispose()
        {
            this.DisposeEventStoreInSingleFile();

            foreach (var sqLiteConnectionWithLock in this.connectionByEventSource.Values)
            {
                sqLiteConnectionWithLock.Dispose();
            }
        }

        #region Obsolete
        [Obsolete("Since v6.0")]
        private void InitializeEventStoreInSingleFile()
        {
            this.connectionStringToEventStoreInSingleFile = this.ToSqliteConnectionString(Path.Combine(this.settings.PathToDatabaseDirectory, "events-data.sqlite3"));

            if (this.fileSystemAccessor.IsFileExists(this.connectionStringToEventStoreInSingleFile))
                this.eventStoreInSingleFile = this.CreateConnection(this.connectionStringToEventStoreInSingleFile);
        }

        [Obsolete("Since v6.0")]
        private IEnumerable<CommittedEvent> ReadFromEventStoreInSingleFile(Guid id, int minVersion, 
            IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return this.eventStoreInSingleFile != null
                ? this.Read(this.eventStoreInSingleFile, id, minVersion, this.backwardCompatibleEventSerializer, progress, cancellationToken)
                : null;
        }

        [Obsolete("Since v6.0")]
        private CommittedEventStream StoreToEventStoreInSingleFile(UncommittedEventStream eventStream)
            => this.eventStoreInSingleFile != null &&
               this.GetTotalEventsByEventSourceId(this.eventStoreInSingleFile, eventStream.SourceId, 0) > 0
                ? this.Store(this.eventStoreInSingleFile, eventStream, this.backwardCompatibleEventSerializer)
                : null;

        [Obsolete("Since v6.0")]
        private void RemoveFromEventStoreInSingleFile(Guid interviewId)
        {
            if (this.eventStoreInSingleFile == null) return;

            this.RemoveEventSourceById(this.eventStoreInSingleFile, interviewId);
        }

        [Obsolete("Since v6.0")]
        private void DisposeEventStoreInSingleFile()
        {
            if (this.eventStoreInSingleFile == null) return;

            var countOfEventsInSingleFile = this.GetTotalEventsInEventStoreInSingleFile();

            this.eventStoreInSingleFile.Dispose();

            if (countOfEventsInSingleFile == 0)
                this.fileSystemAccessor.DeleteFile(this.connectionStringToEventStoreInSingleFile);
        }

        [Obsolete("Since v6.0")]
        protected int GetTotalEventsInEventStoreInSingleFile()
        {
            using (this.eventStoreInSingleFile.Lock())
            {
                return this.eventStoreInSingleFile.Table<EventView>().Count();
            }
        }

        #endregion

        private IEnumerable<CommittedEvent> Read(SQLiteConnectionWithLock connection, Guid id, int minVersion, IEventSerializer serializer, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            var startEventSequence = Math.Max(minVersion, 0);

            var totalEvents = this.GetTotalEventsByEventSourceId(connection, id, startEventSequence);

            if (totalEvents == 0)
                yield break;

            var bulkSize = this.enumeratorSettings.EventChunkSize;

            progress?.Report(new EventReadingProgress(0, totalEvents));

            for (int loadedEvents = 0; loadedEvents < totalEvents; loadedEvents += bulkSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bulk = this.LoadEvents(connection, id, startEventSequence, skip: loadedEvents, take: bulkSize, serializer: serializer);

                for (int eventIndexInBulk = 0; eventIndexInBulk < bulk.Count; eventIndexInBulk++)
                {
                    yield return bulk[eventIndexInBulk];

                    progress?.Report(new EventReadingProgress(loadedEvents + eventIndexInBulk + 1, totalEvents));
                }
            }
        }

        private List<CommittedEvent> LoadEvents(SQLiteConnectionWithLock connection, Guid eventSourceId, int startEventSequence, int skip, int take, IEventSerializer serializer)
        {
            using (connection.Lock())
            {
                return connection
                    .Table<EventView>()
                    .Where(eventView
                        => eventView.EventSourceId == eventSourceId
                        && eventView.EventSequence >= startEventSequence)
                    .OrderBy(x => x.EventSequence)
                    .Skip(skip)
                    .Take(take)
                    .Select(x => ToCommitedEvent(x, serializer))
                    .ToList();
            }
        }

        private int GetTotalEventsByEventSourceId(SQLiteConnectionWithLock connection, Guid eventSourceId, int startEventSequence)
        {
            using (connection.Lock())
            {
                return connection
                    .Table<EventView>()
                    .Count(eventView
                        => eventView.EventSourceId == eventSourceId
                        && eventView.EventSequence >= startEventSequence);
            }
        }

        private CommittedEventStream Store(SQLiteConnectionWithLock connection, UncommittedEventStream eventStream, IEventSerializer serializer)
        {
            using (connection.Lock())
            {
                try
                {
                    connection.BeginTransaction();

                    this.ValidateStreamVersion(connection, eventStream);

                    List<EventView> storedEvents = eventStream.Select(x => ToStoredEvent(x, serializer)).ToList();
                    foreach (var @event in storedEvents)
                    {
                        connection.Insert(@event);
                    }

                    connection.Commit();
                    return new CommittedEventStream(eventStream.SourceId, eventStream.Select(ToCommitedEvent));
                }
                catch
                {
                    connection.Rollback();
                    throw;
                }
            }
        }

        private void ValidateStreamVersion(SQLiteConnectionWithLock connection, UncommittedEventStream eventStream)
        {
            var expectedVersion = eventStream.InitialVersion;
            if (expectedVersion == 0)
            {
                bool viewExists;

                using (connection.Lock())
                    viewExists = connection.Table<EventView>().Any(x => x.EventSourceId == eventStream.SourceId);

                if (viewExists)
                {
                    var errorMessage = $"Wrong version number. Expected to store new event stream, but it already exists. EventStream Id: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                int currentStreamVersion;
                var commandText = $"SELECT MAX({nameof(EventView.EventSequence)}) FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";

                using (connection.Lock())
                {
                    var sqLiteCommand = connection.CreateCommand(commandText, eventStream.SourceId);
                    currentStreamVersion = sqLiteCommand.ExecuteScalar<int>();
                }


                var expectedExistingSequence = eventStream.Min(x => x.EventSequence) - 1;
                if (expectedExistingSequence != currentStreamVersion)
                {
                    var errorMessage = $"Wrong version number. Expected event stream with version {expectedExistingSequence}, but actual {currentStreamVersion}. SourceId: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        private void RemoveEventSourceById(SQLiteConnectionWithLock connection, Guid interviewId)
        {
            using (connection.Lock())
            {
                try
                {
                    connection.BeginTransaction();
                    var commandText = $"DELETE FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";
                    var sqLiteCommand = connection.CreateCommand(commandText, interviewId);
                    sqLiteCommand.ExecuteNonQuery();
                    connection.Commit();
                }
                catch
                {
                    connection.Rollback();
                    throw;
                }
            }
        }

        private static CommittedEvent ToCommitedEvent(EventView storedEvent, IEventSerializer serializer)
            => new CommittedEvent(
                commitId: storedEvent.CommitId ?? storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: serializer.DeserializePayload(storedEvent));

        private static CommittedEvent ToCommitedEvent(UncommittedEvent storedEvent)
            => new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventIdentifier,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.EventTimeStamp,
                globalSequence: -1,
                payload: storedEvent.Payload);

        private static EventView ToStoredEvent(UncommittedEvent evt, IEventSerializer serializer)
            => new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = serializer.SerializeEventPayload(evt),
                EventType = evt.Payload.GetType().Name
            };

        private interface IEventSerializer
        {
            string SerializeEventPayload(UncommittedEvent evnt);
            Infrastructure.EventBus.IEvent DeserializePayload(EventView storedEvent);
        }

        [Obsolete("v 5.10.10")]
        private class BackwardCompatibleEventSerializer : IEventSerializer
        {
            public string SerializeEventPayload(UncommittedEvent evnt)
                => JsonConvert.SerializeObject(evnt.Payload, OldJsonSerializerSettings());


            public Infrastructure.EventBus.IEvent DeserializePayload(EventView storedEvent)
               => JsonConvert.DeserializeObject<Infrastructure.EventBus.IEvent>(storedEvent.JsonEvent, OldJsonSerializerSettings());

            [Obsolete("v 5.10.10")]
            private static readonly Func<JsonSerializerSettings> OldJsonSerializerSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = new CapiAndMainCoreToInterviewerAndSharedKernelsBinder()
            };

            [Obsolete("Since v6.0")]
            private class CapiAndMainCoreToInterviewerAndSharedKernelsBinder : DefaultSerializationBinder
            {
                public override Type BindToType(string assemblyName, string typeName)
                {
                    var oldCapiAssemblyName = "WB.UI.Capi";
                    var newCapiAssemblyName = "WB.Core.BoundedContexts.Interviewer";
                    var newQuestionsAssemblyName = "WB.Core.SharedKernels.Questionnaire";
                    var oldMainCoreAssemblyName = "Main.Core";

                    if (String.Equals(assemblyName, oldCapiAssemblyName, StringComparison.Ordinal))
                    {
                        assemblyName = newCapiAssemblyName;
                    }
                    else if (String.Equals(assemblyName, oldMainCoreAssemblyName, StringComparison.Ordinal))
                    {
                        if (oldMainCoreTypeMap.ContainsKey(typeName))
                            assemblyName = oldMainCoreTypeMap[typeName];
                        else
                            assemblyName = newQuestionsAssemblyName;
                    }

                    return base.BindToType(assemblyName, typeName);
                }

                private readonly Dictionary<string, string> oldMainCoreTypeMap = new Dictionary<string, string>()
            {
                {"Main.Core.Events.AggregateRootEvent", "WB.Core.Infrastructure"},
                {"Main.Core.Events.User.NewUserCreated", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserChanged", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserLocked", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserLockedBySupervisor", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserUnlocked", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserUnlockedBySupervisor", "WB.Core.SharedKernels.DataCollection"},
            };
            }
        }

        private class EventSerializer : IEventSerializer
        {
            private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            private readonly IEventTypeResolver eventTypeResolver;

            public EventSerializer(IEventTypeResolver eventTypeResolver)
            {
                this.eventTypeResolver = eventTypeResolver;
            }

            public string SerializeEventPayload(UncommittedEvent evnt)
                => JsonConvert.SerializeObject(evnt.Payload, Formatting.None, JsonSerializerSettings);

            public Infrastructure.EventBus.IEvent DeserializePayload(EventView storedEvent)
                => JsonConvert.DeserializeObject(storedEvent.JsonEvent, this.eventTypeResolver.ResolveType(storedEvent.EventType), JsonSerializerSettings) as Infrastructure.EventBus.IEvent;
        }
    }
}
