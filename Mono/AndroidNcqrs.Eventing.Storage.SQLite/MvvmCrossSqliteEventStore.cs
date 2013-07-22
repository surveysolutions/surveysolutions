using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

using SQLite;
using WB.Core.Infrastructure.Backup;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IEventStore, IMvxServiceConsumer,IBackupable
    {
        private readonly string folderName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private object locker=new object();
        public MvvmCrossSqliteEventStore(string folderName)
            
        {
            this.folderName = folderName;

            if (!Directory.Exists(FullPathToFolder))
            {
                Directory.CreateDirectory(FullPathToFolder);
            }

            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            connectionFactory = this.GetService<ISQLiteConnectionFactory>();
        }

        private string FullPathToFolder {
            get
            {
               return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                       folderName);
            }
        }

        private void WrapConnection(Guid eventSourceId, Action<ISQLiteConnection> action)
        {
            lock (locker)
            {
                using (var connection = connectionFactory.Create(Path.Combine(folderName, eventSourceId.ToString())))
                {
                    connection.CreateTable<StoredEvent>();
                    action(connection);
                }
            }
        }

        public void Store(UncommittedEventStream eventStream)
        {
            WrapConnection(eventStream.SourceId, (connection) => connection.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true));
        }


        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            IEnumerable<CommittedEvent> events = Enumerable.Empty<CommittedEvent>();
            
            WrapConnection(id, (connection) =>
                {
                    events = ((TableQuery<StoredEvent>) connection.Table<StoredEvent>())
                        .Where(
                            x => x.Sequence >= minVersion &&
                                 x.Sequence <= maxVersion).ToList()
                        .Select(x => x.ToCommitedEvent(id));
                });
            return new CommittedEventStream(id, events);
        }

        public void CleanStream(Guid id)
        {
            File.Delete(
                System.IO.Path.Combine(FullPathToFolder, id.ToString()));
        }

        public string GetPathToBakupFile()
        {
            return FullPathToFolder;
        }

        public void RestoreFromBakupFolder(string path)
        {
            var dirWithImeges = Path.Combine(path, folderName);
            foreach (var file in Directory.EnumerateFiles(FullPathToFolder))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(dirWithImeges))
                File.Copy(file, Path.Combine(FullPathToFolder, Path.GetFileName(file)));
        }
    }
}