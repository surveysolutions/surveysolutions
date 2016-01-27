using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ncqrs.Eventing;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteEventStorage : IInterviewerEventStorage, IDisposable
    {
        private readonly ISerializer serializer;
        private readonly SQLiteConnectionWithLock connection;
        private ILogger logger;

        public SqliteEventStorage(ISQLitePlatform sqLitePlatform, 
            ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            ISerializer serializer,
            ITraceListener traceListener, 
            SqliteSettings settings)
        {
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, "events-data.sqlite3");
            this.connection = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true)))
            {
                TraceListener = traceListener
            };
            this.logger = logger;
            this.serializer = serializer;
            this.connection.CreateTable<EventView>();
            this.connection.CreateIndex<EventView>(entity => entity.EventId);
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            var events = this.connection.Table<EventView>().Where(eventView => eventView.EventSourceId == id &&
                                                                  eventView.EventSequence >= minVersion && eventView.EventSequence <= maxVersion)
                                                            .OrderBy(x => x.EventSequence);
            
            return new CommittedEventStream(id, events.Select(this.ToCommitedEvent));
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            try
            {
                this.connection.BeginTransaction();

                this.ValidateStreamVersion(eventStream);

                var storedEvents = eventStream.Select(this.ToStoredEvent).ToList();
                foreach (var @event in storedEvents)
                {
                    connection.Insert(@event);
                }

                this.connection.Commit();
                return new CommittedEventStream(eventStream.SourceId, storedEvents.Select(this.ToCommitedEvent));
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
                var views = this.connection.Table<EventView>().Where(x => x.EventSourceId == eventStream.SourceId).ToList();
                if (views.Count > 0)
                {
                    var errorMessage = $"Wrong version number. Expected to store new event stream, but it already exists. EventStream Id: {eventStream.SourceId}";
                    this.logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                var commandText = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} = ?", nameof(EventView.EventSequence), nameof(EventView), nameof(EventView.EventSourceId));
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

                var eventViews = this.connection.Table<EventView>().Where(x => x.EventSourceId == interviewId);
                foreach (var eventView in eventViews)
                {
                    this.connection.Delete(eventView);
                }
                this.connection.Commit();
            }
            catch
            {
                this.connection.Rollback();
                throw;
            }
        }

        [Obsolete("Remove when all clients are upgraded to 5.5")]
        public void MigrateOldEvents(IEnumerable<EventView> eventViews)
        {
            foreach (var eventView in eventViews)
            {
                this.connection.Insert(eventView);
            }
        }

        private CommittedEvent ToCommitedEvent(EventView storedEvent)
        {
            return new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: -1,
                payload: this.ToEvent(storedEvent.JsonEvent));
        }

        private EventView ToStoredEvent(UncommittedEvent evt)
        {
            return new EventView
            {
                EventId = evt.EventIdentifier,
                EventSourceId = evt.EventSourceId,
                EventSequence = evt.EventSequence,
                DateTimeUtc = evt.EventTimeStamp,
                JsonEvent = this.serializer.Serialize(evt.Payload, TypeSerializationSettings.AllTypes)
            };
        }

        private Infrastructure.EventBus.IEvent ToEvent(string json)
        {
            var replaceOldAssemblyNames = json.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");
            replaceOldAssemblyNames =
                new[]
                {
                    "NewUserCreated", "UserChanged", "UserLocked", "UserLockedBySupervisor", "UserUnlocked",
                    "UserUnlockedBySupervisor"
                }.Aggregate(replaceOldAssemblyNames,
                    (current, type) =>
                        current.Replace($"Main.Core.Events.User.{type}, Main.Core",
                            $"Main.Core.Events.User.{type}, WB.Core.SharedKernels.DataCollection"));

            return this.serializer.Deserialize<Infrastructure.EventBus.IEvent>(replaceOldAssemblyNames);
        }

        public void Dispose()
        {
            this.connection.Dispose();
        }
    }
}