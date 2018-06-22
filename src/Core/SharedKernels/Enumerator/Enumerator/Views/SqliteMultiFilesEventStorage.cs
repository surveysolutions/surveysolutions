using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class 
        SqliteMultiFilesEventStorage : IEnumeratorEventStorage
    {
        private SQLiteConnectionWithLock eventStoreInSingleFile;
        internal readonly Dictionary<Guid, SQLiteConnectionWithLock> connectionByEventSource = new Dictionary<Guid, SQLiteConnectionWithLock>();
        private readonly SqliteSettings settings;
        private readonly IEnumeratorSettings enumeratorSettings;

        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private string connectionStringToEventStoreInSingleFile;
        private static readonly object lockObject = new object();
        private readonly EventSerializer eventSerializer;
        private BackwardCompatibleEventSerializer backwardCompatibleEventSerializer;

        public SqliteMultiFilesEventStorage(ILogger logger,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings,
            IFileSystemAccessor fileSystemAccessor,
            IEventTypeResolver eventTypesResolver)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
            this.enumeratorSettings = enumeratorSettings;
            this.logger = logger;
            this.eventSerializer = new EventSerializer(eventTypesResolver);

            this.InitializeEventStoreInSingleFile();
        }

        private SQLiteConnectionWithLock CreateConnection(string connectionString)
        {
            var sqConnection = new SQLiteConnectionString(connectionString, true);
            var connection = new SQLiteConnectionWithLock(sqConnection,
                openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

            connection.CreateTable<EventView>();

            return connection;
        }

        private string GetEventSourceConnectionString(Guid eventSourceId)
            => this.ToSqliteConnectionString(Path.Combine(this.settings.PathToInterviewsDirectory, $"{eventSourceId.FormatGuid()}.sqlite3"));

        private string ToSqliteConnectionString(string pathToDatabase)
            => this.settings.InMemoryStorage ? $":memory:" : pathToDatabase;

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

        public int? GetLastEventSequence(Guid id)
        {
            var eventSourceFilePath = this.GetEventSourceConnectionString(id);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                var connection = this.GetOrCreateConnection(id);
                return GetLastEventSequence(connection, id);
            }
            return GetLastEventSequence(this.eventStoreInSingleFile, id);
        }

        private int? GetLastEventSequence(SQLiteConnectionWithLock connection, Guid id)
        {
            using (connection.Lock())
            {
                var isDataExist = connection
                                      .Table<EventView>()
                                      .FirstOrDefault(eventView => eventView.EventSourceId == id) != null;

                if (!isDataExist)
                    return null;

                var lastEventSequence = connection
                    .Table<EventView>()
                    .Where(eventView => eventView.EventSourceId == id)
                    .Max(eventView => eventView.EventSequence);

                return lastEventSequence;
            }
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

        public void StoreEvents(CommittedEventStream events)
        {
            var connection = this.GetOrCreateConnection(events.SourceId);
            using (connection.Lock())
            {
                try
                {
                    connection.RunInTransaction(() =>
                    {
                        var storedEvents = events.Select(x => ToStoredEvent(x, eventSerializer));
                        foreach (var @event in storedEvents)
                        {
                            connection.Insert(@event);
                        }
                    });
                }
                catch (SQLiteException ex)
                {
                    this.logger.Fatal($"Failed to persist eventstream {events.SourceId}", ex);
                    throw;
                }
            }
        }

        public List<CommittedEvent> GetPendingEvents(Guid interviewId)
        {
            IEnumerable<CommittedEvent> events = null;

            var eventSourceFilePath = this.GetEventSourceConnectionString(interviewId);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                var connection = this.GetOrCreateConnection(interviewId);
                
                using (connection.Lock())
                {
                    var committedEvents = connection
                        .Table<EventView>()
                        .Where(eventView
                            => eventView.EventSourceId == interviewId
                               && eventView.ExistsOnHq == null || eventView.ExistsOnHq == 0)
                        .OrderBy(x => x.EventSequence)
                        .ToList()
                        .Select(x => ToCommitedEvent(x, eventSerializer))
                        .ToList();
                    return committedEvents;
                }
            }

            return new List<CommittedEvent>();
        }

        public int GetLastEventKnownToHq(Guid interviewId)
        {
            var connection = this.GetOrCreateConnection(interviewId);
            using (connection.Lock())
            {
                var lastKnownEvent = connection.Table<EventView>()
                    .Where(eventView => eventView.EventSourceId == interviewId && eventView.ExistsOnHq == 1)
                    .OrderByDescending(e => e.EventSequence)
                    .FirstOrDefault();
                return lastKnownEvent?.EventSequence ?? 0;
            }
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
            this.backwardCompatibleEventSerializer = new BackwardCompatibleEventSerializer();

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

        [Obsolete("v 5.10.10")]
        private class BackwardCompatibleEventSerializer : IEventSerializer
        {
            public string Serialize(IEvent @event)
                => JsonConvert.SerializeObject(@event, OldJsonSerializerSettings());


            public IEvent Deserialize(string eventAsString, string eventTypeAsString)
               => JsonConvert.DeserializeObject<IEvent>(eventAsString, OldJsonSerializerSettings());

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
                return connection
                .Table<EventView>()
                .Where(eventView
                    => eventView.EventSourceId == eventSourceId
                       && eventView.EventSequence >= startEventSequence)
                .OrderBy(x => x.EventSequence)
                .Skip(skip)
                .Take(take)
                .ToList()
                .Select(x => ToCommitedEvent(x, serializer))
                .ToList();
        }

        private int GetTotalEventsByEventSourceId(SQLiteConnectionWithLock connection, Guid eventSourceId, int startEventSequence)
        {
            using (connection.Lock())
                return connection
                .Table<EventView>()
                .Count(eventView
                    => eventView.EventSourceId == eventSourceId
                       && eventView.EventSequence >= startEventSequence);
        }

        private CommittedEventStream Store(SQLiteConnectionWithLock connection, UncommittedEventStream eventStream, IEventSerializer serializer)
        {
            using (connection.Lock())
            {
                try
                {
                    connection.RunInTransaction(() =>
                    {
                        this.ValidateStreamVersion(connection, eventStream);

                        List<EventView> storedEvents = eventStream.Select(x => ToStoredEvent(x, serializer)).ToList();
                        foreach (var @event in storedEvents)
                        {
                            connection.Insert(@event);
                        }
                    });
                }
                catch (SQLiteException ex)
                {
                    this.logger.Fatal($"Failed to persist eventstream {eventStream.SourceId}", ex);
                    throw;
                }
            }

            return new CommittedEventStream(eventStream.SourceId, eventStream.Select(ToCommitedEvent));
        }

        private void ValidateStreamVersion(SQLiteConnectionWithLock connection, UncommittedEventStream eventStream)
        {
            var expectedVersion = eventStream.InitialVersion;
            if (expectedVersion == 0)
            {
                bool viewExists;

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
                var sqLiteCommand = connection.CreateCommand(commandText, eventStream.SourceId);
                var scalarValue = sqLiteCommand.ExecuteScalar<string>();
                currentStreamVersion = scalarValue == null ? 0 : Convert.ToInt32(scalarValue);

                var expectedExistingSequence = eventStream.Min(x => x.EventSequence) - 1;
                if (expectedExistingSequence != currentStreamVersion)
                {
                    var errorMessage =
                        $"Wrong version number. Expected event stream with version {expectedExistingSequence}, but actual {currentStreamVersion}. SourceId: {eventStream.SourceId}";
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
                payload: serializer.Deserialize(storedEvent.JsonEvent, storedEvent.EventType));

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
                JsonEvent = serializer.Serialize(evt.Payload),
                EventType = evt.Payload.GetType().Name
            };

        private static EventView ToStoredEvent(CommittedEvent evt, IEventSerializer serializer)
        {
            return new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = serializer.Serialize(evt.Payload),
                EventType = evt.Payload.GetType().Name,
                ExistsOnHq = 1
            };
        }


        private interface IEventSerializer
        {
            string Serialize(IEvent @event);
            IEvent Deserialize(string eventAsString, string eventTypeAsString);
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
                Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() }
            };

            private readonly IEventTypeResolver eventTypesResolver;

            public EventSerializer(IEventTypeResolver eventTypesResolver)
            {
                this.eventTypesResolver = eventTypesResolver;
            }

            public string Serialize(IEvent @event)
                => JsonConvert.SerializeObject(@event, Formatting.None, JsonSerializerSettings);

            public IEvent Deserialize(string eventAsString, string eventTypeAsString)
                => JsonConvert.DeserializeObject(eventAsString, this.eventTypesResolver.ResolveType(eventTypeAsString), JsonSerializerSettings) as IEvent;
        }
    }
}
