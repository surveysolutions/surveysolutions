using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IEventStore, IBackupable
    {
        private readonly string folderName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private readonly object locker = new object();

        public MvvmCrossSqliteEventStore(string folderName)
        {
            this.folderName = folderName;

            if (!Directory.Exists(FullPathToFolder))
            {
                Directory.CreateDirectory(FullPathToFolder);
            }

            PluginLoader.Instance.EnsureLoaded();
            connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
        }

        private string FullPathToFolder 
        {
            get
            {
               return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folderName);
            }
        }

        private void WrapConnection(Guid eventSourceId, Action<ISQLiteConnection> action)
        {
            lock (locker)
            {
                using (var connection = connectionFactory.Create(Path.Combine(FullPathToFolder, eventSourceId.ToString())))
                {
                    connection.CreateTable<StoredEvent>();
                    action(connection);
                }
            }
        }

        public void Store(UncommittedEventStream eventStream)
        {
            WrapConnection(eventStream.SourceId, connection => connection.InsertAll(eventStream.Select(x => x.ToStoredEvent())));
        }

        public CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion)
        {
            IEnumerable<CommittedEvent> events = Enumerable.Empty<CommittedEvent>();
            
            WrapConnection(id, connection =>
            {
                events = connection.Table<StoredEvent>()
                    .Where(
                        x => x.Sequence >= minVersion &&
                                x.Sequence <= maxVersion).ToList()
                    .Select(x => x.ToCommitedEvent(id));
            });

            return new CommittedEventStream(id, events);
        }

        public void CleanStream(Guid id)
        {
            var file = Path.Combine(FullPathToFolder, id.ToString());
            if (File.Exists(file)) File.Delete(file);
        }

        public string GetPathToBackupFile()
        {
            return FullPathToFolder;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithImages = Path.Combine(path, folderName);
           
            foreach (var file in Directory.EnumerateFiles(FullPathToFolder))
            {
                File.Delete(file);
            } 
            
            if (!Directory.Exists(dirWithImages))
                return;

            foreach (var file in Directory.GetFiles(dirWithImages))
                File.Copy(file, Path.Combine(FullPathToFolder, Path.GetFileName(file)));
        }
    }
}