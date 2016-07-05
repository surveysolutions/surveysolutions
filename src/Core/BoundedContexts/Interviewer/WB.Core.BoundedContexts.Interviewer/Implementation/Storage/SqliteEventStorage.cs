using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite.Net;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteEventStorage
    {
        private readonly IEnumeratorSettings enumeratorSettings;
        private readonly ILogger logger;

        public SqliteEventStorage(
            ILogger logger,
            IEnumeratorSettings enumeratorSettings)
        {
            this.logger = logger;
            this.enumeratorSettings = enumeratorSettings;
        }

        protected IEnumerable<CommittedEvent> Read(SQLiteConnectionWithLock connection, Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
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

                var bulk = this.LoadEvents(connection, id, startEventSequence, skip: loadedEvents, take: bulkSize);

                for (int eventIndexInBulk = 0; eventIndexInBulk < bulk.Count; eventIndexInBulk++)
                {
                    yield return bulk[eventIndexInBulk];

                    progress?.Report(new EventReadingProgress(loadedEvents + eventIndexInBulk + 1, totalEvents));
                }
            }
        }

        private List<CommittedEvent> LoadEvents(SQLiteConnectionWithLock connection, Guid eventSourceId, int startEventSequence, int skip, int take)
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
                    .Select(ToCommitedEvent)
                    .ToList();
            }
        }

        protected int GetTotalEventsByEventSourceId(SQLiteConnectionWithLock connection, Guid eventSourceId, int startEventSequence)
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

        protected CommittedEventStream Store(SQLiteConnectionWithLock connection, UncommittedEventStream eventStream)
        {
            using (connection.Lock())
            {
                try
                {
                    connection.BeginTransaction();

                    this.ValidateStreamVersion(connection, eventStream);

                    List<EventView> storedEvents = eventStream.Select(ToStoredEvent).ToList();
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

        protected virtual void RemoveEventSourceById(SQLiteConnectionWithLock connection, Guid interviewId)
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

        protected static CommittedEvent ToCommitedEvent(EventView storedEvent)
            => new CommittedEvent(
                commitId: storedEvent.CommitId ?? storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: JsonConvert.DeserializeObject<Infrastructure.EventBus.IEvent>(storedEvent.JsonEvent, JsonSerializerSettings()));

        protected static CommittedEvent ToCommitedEvent(UncommittedEvent storedEvent)
            => new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventIdentifier,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.EventTimeStamp,
                globalSequence: -1,
                payload: storedEvent.Payload);

        protected static EventView ToStoredEvent(UncommittedEvent evt)
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
        
        internal static Func<JsonSerializerSettings> JsonSerializerSettings = () => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Binder = new CapiAndMainCoreToInterviewerAndSharedKernelsBinder()
        };

        [Obsolete("Since v6.0")]
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
