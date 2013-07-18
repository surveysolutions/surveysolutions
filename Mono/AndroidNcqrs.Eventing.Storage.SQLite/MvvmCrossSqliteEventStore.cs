using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

using SQLite;
using WB.Core.Infrastructure.Backup;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IEventStore, IBackupable
    {
        private  ISQLiteConnection _connection;
        private readonly string databaseName;
        public MvvmCrossSqliteEventStore(string databaseName)
        {
            this.databaseName = databaseName;
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(databaseName);
            _connection.CreateTable<StoredEvent>();

        }


        public void Store(UncommittedEventStream eventStream)
        {
            _connection.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true);
        }


        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var idString = id.ToString();
            return new CommittedEventStream(id, ((ITableQuery<StoredEvent>) _connection.Table<StoredEvent>())
                                                .Where(
                                                    x =>
                                                    x.EventSourceId == idString && x.Sequence >= minVersion &&
                                                    x.Sequence <= maxVersion).ToList()
                                                .Select(x => x.ToCommitedEvent()));
        }

        public void CleanStream(Guid id)
        {
            _connection.Execute("delete from StoredEvent where EventSourceId = ?", id.ToString());
        }

        public string GetPathToBakupFile()
        {
            return this._connection.DatabasePath;
        }

        public void RestoreFromBakupFolder(string path)
        {
            _connection.Close();

            File.Copy(Path.Combine(path, databaseName),
                      System.IO.Path.Combine(
                          System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                          this.databaseName), true);
            var connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(databaseName);
        }
    }
}