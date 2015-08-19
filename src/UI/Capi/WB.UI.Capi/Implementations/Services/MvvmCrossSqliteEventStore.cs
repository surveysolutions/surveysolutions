using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.WriteSide;
using WB.UI.Capi.Extensions;

namespace WB.UI.Capi.Implementations.Services
{
    public class MvvmCrossSqliteEventStore : IEventStore, IBackupable, IWriteSideCleaner
    {
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

        public void Store(UncommittedEventStream eventStream)
        {
            this.WrapConnection(eventStream.SourceId, connection => connection.InsertAll(eventStream.Select(x => x.ToStoredEvent())));
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
                    .Select(x => x.ToCommitedEvent(id));
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
    }
}