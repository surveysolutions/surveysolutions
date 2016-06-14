using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using SQLite.Net.Async;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteEventStorage : IInterviewerEventStorage, IDisposable
    {
        private IEnumeratorSettings enumeratorSettings;
        private readonly SQLiteConnectionWithLock connection;
        private ILogger logger;

        static readonly Encoding TextEncoding = Encoding.UTF8;

        public SqliteEventStorage(ISQLitePlatform sqLitePlatform,
            ILogger logger,
            ITraceListener traceListener,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings)
        {
            string pathToDatabase = settings.PathToDatabaseDirectory;
            if (pathToDatabase != ":memory:")
            {
                pathToDatabase = Path.Combine(settings.PathToDatabaseDirectory, "events-data.sqlite3");
            }

            this.connection = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true,
                    new BlobSerializerDelegate(
                        (obj) => TextEncoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None)),
                        (data, type) => JsonConvert.DeserializeObject(TextEncoding.GetString(data, 0, data.Length), type),
                        (type) => true)))
            {
                //TraceListener = traceListener
            };

            this.logger = logger;
            this.enumeratorSettings = enumeratorSettings;
            this.connection.CreateTable<EventView>();
            this.connection.CreateIndex<EventView>(entity => entity.EventId);
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
            => this.Read(id, minVersion, null, CancellationToken.None);

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            var totalEventCount = this
                .connection
                .Table<EventView>()
                .Count(eventView
                    => eventView.EventSourceId == id
                       && eventView.EventSequence >= minVersion);

            if (totalEventCount == 0)
                yield break;
            
            int lastReadEventSequence = Math.Max(minVersion, 0);
            var bulkSize = this.enumeratorSettings.EventChunkSize;
            List<CommittedEvent> bulk;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var startSequenceInTheBulk = lastReadEventSequence;
                var endSequenceInTheBulk = startSequenceInTheBulk + bulkSize;
                bulk = this
                    .connection
                    .Table<EventView>()
                    .Where(eventView
                        => eventView.EventSourceId == id
                        && eventView.EventSequence >= startSequenceInTheBulk
                        && eventView.EventSequence < endSequenceInTheBulk)
                    .OrderBy(x => x.EventSequence)
                    .Select(ToCommitedEvent)
                    .ToList();

                foreach (var committedEvent in bulk)
                {
                    yield return committedEvent;
                    lastReadEventSequence = committedEvent.EventSequence + 1;
                    progress?.Report(new EventReadingProgress(lastReadEventSequence, totalEventCount));
                }

            } while (bulk.Count > 0);
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            try
            {
                this.connection.BeginTransaction();

                this.ValidateStreamVersion(eventStream);

                List<EventView> storedEvents = eventStream.Select(this.ToStoredEvent).ToList();
                foreach (var @event in storedEvents)
                {
                    connection.Insert(@event);
                }

                this.connection.Commit();
                return new CommittedEventStream(eventStream.SourceId, eventStream.Select(this.ToCommitedEvent));
            }
            catch
            {
                this.connection.Rollback();
                throw;
            }
        }

        private void ValidateStreamVersion(UncommittedEventStream eventStream)
        {
            var expectedVersion = eventStream.InitialVersion;
            if (expectedVersion == 0)
            {
                var viewExists = this.connection.Table<EventView>().Any(x => x.EventSourceId == eventStream.SourceId);
                if (viewExists)
                {
                    var errorMessage = $"Wrong version number. Expected to store new event stream, but it already exists. EventStream Id: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                var commandText = $"SELECT MAX({nameof(EventView.EventSequence)}) FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";
                var sqLiteCommand = this.connection.CreateCommand(commandText, eventStream.SourceId);
                int currentStreamVersion = sqLiteCommand.ExecuteScalar<int>();

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
            try
            {
                this.connection.BeginTransaction();
                var commandText = $"DELETE FROM {nameof(EventView)} WHERE {nameof(EventView.EventSourceId)} = ?";
                var sqLiteCommand = this.connection.CreateCommand(commandText, interviewId);
                sqLiteCommand.ExecuteNonQuery();
                this.connection.Commit();
            }
            catch
            {
                this.connection.Rollback();
                throw;
            }
        }

        private static CommittedEvent ToCommitedEvent(EventView storedEvent)
        {
            return new CommittedEvent(
                commitId: storedEvent.CommitId ?? storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: JsonConvert.DeserializeObject<Infrastructure.EventBus.IEvent>(storedEvent.JsonEvent, JsonSerializerSettings()));
        }

        private CommittedEvent ToCommitedEvent(UncommittedEvent storedEvent)
        {
            return new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventIdentifier,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.EventTimeStamp,
                globalSequence: -1,
                payload: storedEvent.Payload);
        }

        private EventView ToStoredEvent(UncommittedEvent evt)
        {
            return new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                CommitId = evt.CommitId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = JsonConvert.SerializeObject(evt.Payload, JsonSerializerSettings())
            };
        }

        public void Dispose()
        {
            this.connection.Dispose();
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
