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
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteEventStorage : IInterviewerEventStorage, IDisposable
    {
        private IEnumeratorSettings enumeratorSettings;
        

        internal Dictionary<Guid, SQLiteConnectionWithLock> connections = new Dictionary<Guid, SQLiteConnectionWithLock>();
        private ILogger logger;
        private SqliteSettings settings;
        private ISQLitePlatform sqLitePlatform;
        private ITraceListener traceListener;

        private const string eventStoreDBNameFormat = "{0}-events.sqlite3";
        private static readonly Object creatorLock = new Object();
        
        static readonly Encoding TextEncoding = Encoding.UTF8;

        public SqliteEventStorage(ISQLitePlatform sqLitePlatform,
            ILogger logger,
            ITraceListener traceListener,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings)
        {
            this.logger = logger;
            this.enumeratorSettings = enumeratorSettings;
            this.settings = settings;
            this.sqLitePlatform = sqLitePlatform;
            this.traceListener = traceListener;
        }

        private SQLiteConnectionWithLock InitConnection(string DBPath)
        {
            string pathToDatabase = this.settings.PathToDatabaseDirectory;
            if (pathToDatabase != ":memory:")
            {
                pathToDatabase = DBPath;
            }

            var connection = new SQLiteConnectionWithLock(this.sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true,
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

        private string GetDBPath(Guid eventSourceId)
        {
            return Path.Combine(this.settings.PathToDatabaseDirectory, string.Format(eventStoreDBNameFormat, eventSourceId));
        }

        private SQLiteConnectionWithLock GetOrCreateConnection(Guid eventSourceId)
        {
            SQLiteConnectionWithLock connection;

            if (!connections.TryGetValue(eventSourceId, out connection))
            {
                lock (creatorLock)
                {
                    connection = InitConnection(GetDBPath( eventSourceId));
                    connections.Add(eventSourceId, connection);
                }
            }

            return connection;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
            => this.Read(id, minVersion, null, CancellationToken.None);

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            var startEventSequence = Math.Max(minVersion, 0);

            var totalEvents = this.CountTotalEvents(id, startEventSequence);

            if (totalEvents == 0)
                yield break;

            int readEventCount = 0;

            var connection = GetOrCreateConnection(id);

            var bulkSize = this.enumeratorSettings.EventChunkSize;

            progress?.Report(new EventReadingProgress(readEventCount, totalEvents));

            for (int skipEvents = 0; skipEvents < totalEvents; skipEvents += bulkSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<CommittedEvent> bulk;
                using (connection.Lock())
                {
                    bulk = connection
                        .Table<EventView>()
                        .Where(eventView
                            => eventView.EventSourceId == id
                            && eventView.EventSequence >= startEventSequence)
                        .OrderBy(x => x.EventSequence)
                        .Skip(skipEvents)
                        .Take(bulkSize)
                        .Select(ToCommitedEvent)
                        .ToList();
                }

                foreach (var committedEvent in bulk)
                {
                    yield return committedEvent;
                    readEventCount++;
                    progress?.Report(new EventReadingProgress(readEventCount, totalEvents));
                }
            }
        }

        private int CountTotalEvents(Guid eventSourceId, int startEventSequence)
        {
            var connection = this.GetOrCreateConnection(eventSourceId);

            using (connection.Lock())
            {
                return connection
                    .Table<EventView>()
                    .Count(eventView
                        => eventView.EventSourceId == eventSourceId
                        && eventView.EventSequence >= startEventSequence);
            }
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            var id = eventStream.SourceId;
            var currentConnection = GetOrCreateConnection(id);
            using (currentConnection.Lock())
            {
                try
                {
                    currentConnection.BeginTransaction();

                    this.ValidateStreamVersion(eventStream);

                    List<EventView> storedEvents = eventStream.Select(ToStoredEvent).ToList();
                    foreach (var @event in storedEvents)
                    {
                        currentConnection.Insert(@event);
                    }

                    currentConnection.Commit();
                    return new CommittedEventStream(eventStream.SourceId, eventStream.Select(ToCommitedEvent));
                }
                catch
                {
                    currentConnection.Rollback();
                    throw;
                }
            }
        }

        private void ValidateStreamVersion(UncommittedEventStream eventStream)
        {
            var expectedVersion = eventStream.InitialVersion;
            var currentConnection = GetOrCreateConnection(eventStream.SourceId);
            if (expectedVersion == 0)
            {
                bool viewExists;
                
                using (currentConnection.Lock())
                    viewExists = currentConnection.Table<EventView>().Any(x => x.EventSourceId == eventStream.SourceId);

                if (viewExists)
                {
                    var errorMessage = $"Wrong version number. Expected to store new event stream, but it already exists. EventStream Id: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                int currentStreamVersion = GetMaxEventSequenceByIdImpl(eventStream.SourceId, currentConnection);

                var expectedExistingSequence = eventStream.Min(x => x.EventSequence) - 1;
                if (expectedExistingSequence != currentStreamVersion)
                {
                    var errorMessage = $"Wrong version number. Expected event stream with version {expectedExistingSequence}, but actual {currentStreamVersion}. SourceId: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        public void RemoveEventSourceById(Guid interviewId)
        {
            var currentConnection = GetOrCreateConnection(interviewId);
            
            using (currentConnection.Lock())
            {
                try
                {
                    currentConnection.BeginTransaction();
                    var commandText = $"DELETE FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";
                    var sqLiteCommand = currentConnection.CreateCommand(commandText, interviewId);
                    sqLiteCommand.ExecuteNonQuery();
                    currentConnection.Commit();
                }
                catch
                {
                    currentConnection.Rollback();
                    throw;
                }
            }
        }

        public int GetMaxEventSequenceById(Guid interviewId)
        {
            var currentConnection = GetOrCreateConnection(interviewId);
            return this.GetMaxEventSequenceByIdImpl(interviewId, currentConnection);
        }

        public int GetMaxEventSequenceByIdImpl(Guid interviewId, SQLiteConnectionWithLock currentConnection)
        {
            int currentStreamVersion;
            var commandText = $"SELECT MAX({nameof(EventView.EventSequence)}) FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";

            using (currentConnection.Lock())
            {
                var sqLiteCommand = currentConnection.CreateCommand(commandText, interviewId);
                currentStreamVersion = sqLiteCommand.ExecuteScalar<int>();
            }

            return currentStreamVersion;
        }

        private static CommittedEvent ToCommitedEvent(EventView storedEvent)
            => new CommittedEvent(
                commitId: storedEvent.CommitId ?? storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: JsonConvert.DeserializeObject<Infrastructure.EventBus.IEvent>(storedEvent.JsonEvent, JsonSerializerSettings()));

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

        private static EventView ToStoredEvent(UncommittedEvent evt)
            => new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = JsonConvert.SerializeObject(evt.Payload, JsonSerializerSettings()),
                EventType = evt.Payload.GetType().Name
            };

        public void Dispose()
        {
            foreach (var sqLiteConnectionWithLock in this.connections.Values)
            {
                sqLiteConnectionWithLock.Dispose();
            }
        }

        internal static Func<JsonSerializerSettings> JsonSerializerSettings = () => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Binder = new CapiAndMainCoreToInterviewerAndSharedKernelsBinder()
        };

        [Obsolete("Resolves old namespaces. Cuold be dropped after incompatibility shift with the next version.")]
        internal class CapiAndMainCoreToInterviewerAndSharedKernelsBinder : DefaultSerializationBinder
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
}
