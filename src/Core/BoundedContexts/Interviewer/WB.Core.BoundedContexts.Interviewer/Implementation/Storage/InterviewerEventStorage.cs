using System;
using System.IO;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class InterviewerEventStorage : IEventStore, IDisposable
    {
        private readonly ISerializer serializer;
        private readonly SQLiteConnectionWithLock connection;
        private ILogger logger;

        public InterviewerEventStorage(ISQLitePlatform sqLitePlatform, 
            ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            ISerializer serializer, 
            SqliteSettings settings)
        {
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, "data.mdb");
            this.connection = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true)));
            this.logger = logger;
            this.serializer = serializer;
            this.connection.CreateTable<EventView>();
            this.connection.CreateIndex<EventView>(entity => entity.Id);
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            var events = this.connection.Table<EventView>().Where(eventView => eventView.EventSourceId == id &&
                                                                  eventView.EventSequence >= minVersion && eventView.EventSequence <= maxVersion);
            
            return new CommittedEventStream(id, events.Select(this.ToCommitedEvent));
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            try
            {
                this.connection.BeginTransaction();

                var expectedStreamVersion = eventStream.InitialVersion;
                var sqLiteCommand = this.connection.CreateCommand("SELECT MAX(EventSequence) FROM EventView WHERE EventSourceId = ?", eventStream.SourceId);
                int currentStreamVersion = sqLiteCommand.ExecuteScalar<int>();

                if (expectedStreamVersion != currentStreamVersion)
                {
                    throw new InvalidOperationException($"Wrong version number. Expected event stream with version {expectedStreamVersion}, but actual {currentStreamVersion}");
                }

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

        private CommittedEvent ToCommitedEvent(EventView storedEvent)
        {
            return new CommittedEvent(
                commitId: storedEvent.EventSourceId,
                origin: string.Empty,
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                globalSequence: storedEvent.OID,
                payload: this.ToEvent(storedEvent.JsonEvent));
        }

        private EventView ToStoredEvent(UncommittedEvent evt)
        {
            return new EventView
            {
                Id = evt.EventIdentifier.FormatGuid(),
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