using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class SqliteMultiFilesEventStorage : IEnumeratorEventStorage, IDisposable
    {
        internal readonly Dictionary<Guid, SQLiteConnectionWithLock> connectionByEventSource = new Dictionary<Guid, SQLiteConnectionWithLock>();
        private readonly SqliteSettings settings;
        private readonly IEnumeratorSettings enumeratorSettings;

        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IEncryptionService encryptionService;
        private readonly IWorkspaceAccessor workspaceAccessor;

        private static readonly object lockObject = new object();
        private readonly EventSerializer eventSerializer;

        public SqliteMultiFilesEventStorage(ILogger logger,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings,
            IFileSystemAccessor fileSystemAccessor,
            IEventTypeResolver eventTypesResolver,
            IEncryptionService encryptionService,
            IWorkspaceAccessor workspaceAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.encryptionService = encryptionService;
            this.workspaceAccessor = workspaceAccessor;
            this.settings = settings;
            this.enumeratorSettings = enumeratorSettings;
            this.logger = logger;
            this.eventSerializer = new EventSerializer(eventTypesResolver);
        }

        private SQLiteConnectionWithLock CreateConnection(string connectionString)
        {
            var directory = fileSystemAccessor.GetDirectory(connectionString);
            if (!fileSystemAccessor.IsDirectoryExists(directory))
                fileSystemAccessor.CreateDirectory(directory);
            
            var sqConnection = new SQLiteConnectionString(connectionString, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);
            var connection = new SQLiteConnectionWithLock(sqConnection);

            connection.CreateTable<EventView>();

            return connection;
        }

        private string GetEventSourceConnectionString(Guid eventSourceId)
            => this.ToSqliteConnectionString(Path.Combine(
                this.settings.PathToRootDirectory, 
                workspaceAccessor.GetCurrentWorkspaceName(), 
                this.settings.DataDirectoryName, 
                this.settings.InterviewsDirectoryName, 
                $"{eventSourceId.FormatGuid()}.sqlite3"));

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
                        var eventSourceConnectionString = this.GetEventSourceConnectionString(eventSourceId);
                        connection = this.CreateConnection(eventSourceConnectionString);
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
            return this.Read(this.GetOrCreateConnection(id), id, minVersion, this.eventSerializer, progress, cancellationToken) ?? new CommittedEvent[0];
        }

        public int? GetLastEventSequence(Guid id)
        {
            var connection = this.GetOrCreateConnection(id);
            return GetLastEventSequence(connection, id);
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
            => this.Store(this.GetOrCreateConnection(eventStream.SourceId), eventStream, this.eventSerializer);

        public void RemoveEventSourceById(Guid interviewId)
        {
            lock (lockObject)
            {
                var eventSourceFilePath = this.GetEventSourceConnectionString(interviewId);
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

        public void InsertEventsFromHqInEventsStream(Guid interviewId, CommittedEventStream events)
        {
            var connection = this.GetOrCreateConnection(events.SourceId);
            using (connection.Lock())
            {
                try
                {
                    connection.RunInTransaction(() =>
                    {
                        var lastHqEvent = connection.Table<EventView>()
                            .Where(eventView => eventView.EventSourceId == interviewId && eventView.ExistsOnHq == 1)
                            .OrderByDescending(e => e.EventSequence)
                            .FirstOrDefault();

                        var localEvents = connection.Table<EventView>()
                            .Where(eventView => eventView.EventSourceId == interviewId && (eventView.ExistsOnHq == null || eventView.ExistsOnHq != 1))
                            .OrderByDescending(e => e.EventSequence);

                        var eventsCount = events.Count;
                        foreach (var localEvent in localEvents)
                        {
                            localEvent.EventSequence += eventsCount;
                            connection.Update(localEvent);
                        }

                        var storedEvents = events.Select(x => ToStoredEvent(x, eventSerializer)).ToList();
                        for (int i = 0; i < storedEvents.Count; i++)
                        {
                            var storedEvent = storedEvents[i];
                            storedEvent.EventSequence = lastHqEvent.EventSequence + i + 1;
                            connection.Insert(storedEvent);
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

        public bool HasEventsWithoutHqFlag(Guid eventSourceId)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var @event = connection
                    .Table<EventView>()
                    .FirstOrDefault(eventView => eventView.EventSourceId == eventSourceId
                                                 && (eventView.ExistsOnHq == null || eventView.ExistsOnHq != 1));
                return @event != null;
            }
        }


        public bool IsLastEventInSequence(Guid eventSourceId, Guid eventId)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var @eventById = connection
                    .Table<EventView>()
                    .FirstOrDefault(ev => ev.EventId == eventId
                                          && ev.EventSourceId == eventSourceId);

                if (eventById == null)
                    return false;

                var @anyFreshEvent = connection
                    .Table<EventView>()
                    .FirstOrDefault(ev => ev.EventSequence > @eventById.EventSequence
                                          && ev.EventSourceId == eventSourceId);
                return @anyFreshEvent != null;
            }
        }

        public Guid? GetLastEventIdUploadedToHq(Guid eventSourceId)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var @event = connection
                    .Table<EventView>()
                    .Where(ev => ev.EventSourceId == eventSourceId && ev.ExistsOnHq == 1)
                    .OrderByDescending(ev => ev.EventSequence)
                    .FirstOrDefault();

                return @event?.EventId;
            }
        }
        
        public Guid? GetFirstEventId(Guid eventSourceId)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var @event = connection
                    .Table<EventView>()
                    .Where(ev => ev.EventSourceId == eventSourceId)
                    .OrderBy(ev => ev.EventSequence)
                    .FirstOrDefault();

                return @event?.EventId;
            }
        }

        public List<CommittedEvent> GetPendingEvents(Guid interviewId)
        {
            var eventSourceFilePath = this.GetEventSourceConnectionString(interviewId);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                var connection = this.GetOrCreateConnection(interviewId);
                
                using (connection.Lock())
                {
                    var eventViews = connection
                        .Table<EventView>()
                        .Where(eventView
                            => eventView.EventSourceId == interviewId
                               && eventView.ExistsOnHq == null || eventView.ExistsOnHq == 0)
                        .OrderBy(x => x.EventSequence)
                        .ToList();

                    var committedEvents = eventViews
                        .Select(x => ToCommitedEvent(x, eventSerializer, this.encryptionService))
                        .ToList();
                    return committedEvents;
                }
            }

            return new List<CommittedEvent>();
        }

        public bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId,
            params string[] typeNames)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var @event = connection
                    .Table<EventView>()
                    .FirstOrDefault(ev => ev.EventSequence > sequence 
                                          && ev.EventSourceId == eventSourceId 
                                          && typeNames.Contains(ev.EventType));
                return @event != null;
            }
        }

        public CommittedEvent GetEventByEventSequence(Guid eventSourceId, int eventSequence)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var eventV = connection
                    .Table<EventView>()
                    .FirstOrDefault(eventView => eventView.EventSourceId == eventSourceId
                                                 && eventView.EventSequence == eventSequence);

                return eventV == null ? null: ToCommitedEvent(eventV, eventSerializer, this.encryptionService);
            }
        }

        public int GetMaxSequenceForAnyEvent(Guid eventSourceId, params string[] typeNames)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);
            using (connection.Lock())
            {
                var sequence = connection
                    .Table<EventView>()
                    .Where(ev => ev.EventSourceId == eventSourceId
                                 && typeNames.Contains(ev.EventType))
                    .Max(ev => ev.EventSequence);
                return sequence;
            }
        }

        public List<Guid> GetListOfAllItemsIds()
        {
            var pathToInterviewsDirectory = fileSystemAccessor.CombinePath(
                this.settings.PathToRootDirectory,
                workspaceAccessor.GetCurrentWorkspaceName(),
                this.settings.DataDirectoryName, 
                this.settings.InterviewsDirectoryName);
            if (!Directory.Exists(pathToInterviewsDirectory))
                return new List<Guid>();

            Guid item = Guid.Empty;
            var files = Directory.GetFiles(pathToInterviewsDirectory, "*.sqlite3")
                .Select(fn => Path.GetFileNameWithoutExtension(fn))
                .Where(x => Guid.TryParse(x, out item)).Select(x => item).ToList();

            return files;
        }

        public void MarkAllEventsAsReceivedByHq(Guid interviewId)
        {
            var connection = this.GetOrCreateConnection(interviewId);
            using (connection.Lock())
            {
                try
                {
                    connection.BeginTransaction();
                    var commandText = $"UPDATE {nameof(EventView)} " +
                                      $"SET {nameof(EventView.ExistsOnHq)} = 1 " +
                                      $"WHERE ({nameof(EventView.ExistsOnHq)} != 1 OR {nameof(EventView.ExistsOnHq)} is NULL)" +
                                      $" AND {nameof(EventView.EventSourceId)} = ?";
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
            foreach (var sqLiteConnectionWithLock in this.connectionByEventSource.Values)
            {
                sqLiteConnectionWithLock.Dispose();
            }
        }

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
                .Select(x => ToCommitedEvent(x, serializer, this.encryptionService))
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

                        var storedEvents = eventStream.Select(x => ToStoredEvent(x, serializer));

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

        private static CommittedEvent ToCommitedEvent(EventView storedEvent, IEventSerializer serializer, IEncryptionService encryptionService)
            => new CommittedEvent(
                commitId: storedEvent.CommitId ?? storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: serializer.Deserialize(encryptionService.Decrypt(storedEvent.EncryptedJsonEvent), storedEvent.EventType));

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

        private EventView ToStoredEvent(UncommittedEvent evt, IEventSerializer serializer)
            => new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                EncryptedJsonEvent = this.encryptionService.Encrypt(serializer.Serialize(evt.Payload)),
                EventType = evt.Payload.GetType().Name
            };

        private EventView ToStoredEvent(CommittedEvent evt, IEventSerializer serializer)
            => new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                EncryptedJsonEvent = this.encryptionService.Encrypt(serializer.Serialize(evt.Payload)),
                EventType = evt.Payload.GetType().Name,
                ExistsOnHq = 1
            };

        public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
        {
            return GetLastEventSequence(eventSourceId) == lastKnownEventSequence;
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
