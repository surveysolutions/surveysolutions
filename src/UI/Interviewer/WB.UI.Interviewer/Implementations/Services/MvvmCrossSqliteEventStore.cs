using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.WriteSide;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class MvvmCrossSqliteEventStore : IEventStore, IBackupable, IWriteSideCleaner
    {
        private const int GlobalSequence = -1;
        private readonly string folderName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private readonly object locker = new object();

        public MvvmCrossSqliteEventStore(string folderName, IWriteSideCleanerRegistry writeSideCleanerRegistry)
        {
            this.folderName = folderName;

            if (!Directory.Exists(this.FullPathToFolder))
            {
                Directory.CreateDirectory(this.FullPathToFolder);
            }

            PluginLoader.Instance.EnsureLoaded();
            this.connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
            writeSideCleanerRegistry.Register(this);
        }

        private string FullPathToFolder 
        {
            get
            {
               return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), this.folderName);
            }
        }

        private void WrapConnection(Guid eventSourceId, Action<ISQLiteConnection> action)
        {
            lock (this.locker)
            {
                using (var connection = this.connectionFactory.Create(Path.Combine(this.FullPathToFolder, eventSourceId.ToString())))
                {
                    connection.CreateTable<StoredEvent>();
                    action(connection);
                }
            }
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            List<StoredEvent> storedEvents = eventStream.Select(ToStoredEvent).ToList();
            this.WrapConnection(eventStream.SourceId, connection => connection.InsertAll(storedEvents));

            return new CommittedEventStream(eventStream.SourceId, storedEvents.Select(x => ToCommitedEvent(x, eventStream.SourceId)));
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            IEnumerable<CommittedEvent> events = Enumerable.Empty<CommittedEvent>();
            
            this.WrapConnection(id, connection =>
            {
                events = connection.Table<StoredEvent>()
                    .Where(
                        x => x.Sequence >= minVersion &&
                                x.Sequence <= maxVersion).ToList()
                    .Select(x => ToCommitedEvent(x, id));
            });

            return new CommittedEventStream(id, events);
        }

        public string GetPathToBackupFile()
        {
            return this.FullPathToFolder;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithImages = Path.Combine(path, this.folderName);
           
            foreach (var file in Directory.EnumerateFiles(this.FullPathToFolder))
            {
                File.Delete(file);
            } 
            
            if (!Directory.Exists(dirWithImages))
                return;

            foreach (var file in Directory.GetFiles(dirWithImages))
                File.Copy(file, Path.Combine(this.FullPathToFolder, Path.GetFileName(file)));
        }

        public void Clean(Guid aggregateId)
        {
            var file = Path.Combine(this.FullPathToFolder, aggregateId.ToString());
            if (File.Exists(file)) File.Delete(file);
        }

        private static CommittedEvent ToCommitedEvent(StoredEvent storedEvent, Guid eventSourceId)
        {
            var eventTimeStamp = DateTime.FromBinary(storedEvent.TimeStamp);
            return new CommittedEvent(Guid.Parse(storedEvent.CommitId), storedEvent.Origin, Guid.Parse(storedEvent.EventId),
                                      eventSourceId, storedEvent.Sequence,
                                      eventTimeStamp,
                                      GlobalSequence,
                                      GetObject(storedEvent.Data));
        }

        private static StoredEvent ToStoredEvent(UncommittedEvent evt)
        {
            return new StoredEvent(evt.CommitId, evt.Origin, evt.EventIdentifier, evt.EventSequence, evt.EventTimeStamp, evt.Payload);
        }

        private static IEvent GetObject(string json)
        {
            var replaceOldAssemblyNames = json.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");
            foreach (var type in new[] { "NewUserCreated", "UserChanged", "UserLocked", "UserLockedBySupervisor", "UserUnlocked", "UserUnlockedBySupervisor" })
            {
                replaceOldAssemblyNames = replaceOldAssemblyNames.Replace(
                    string.Format("Main.Core.Events.User.{0}, Main.Core", type),
                    string.Format("Main.Core.Events.User.{0}, WB.Core.SharedKernels.DataCollection", type));
            }

            return JsonConvert.DeserializeObject(replaceOldAssemblyNames,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                }) as IEvent;
        }
    }
}