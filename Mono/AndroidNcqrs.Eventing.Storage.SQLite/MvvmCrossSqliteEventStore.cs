using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database.Sqlite;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot.EventStores;
using SQLite;
using SQLiteException = Android.Database.Sqlite.SQLiteException;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IStreamableEventStore, ISnapshootEventStore, IMvxServiceConsumer
    {
        private readonly ISQLiteConnection _connection;

        public MvvmCrossSqliteEventStore(string databaseName)
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(databaseName);
            //  _connection = connectionFactory.Create("EventStore");
            _connection.CreateTable<StoredEvent>();

        }

        // private readonly  ISQLiteConnectionFactory _connectionFactory;


        public Ncqrs.Eventing.CommittedEvent GetLatestSnapshoot(Guid id)
        {
            var idString = id.ToString();
            return ((TableQuery<StoredEvent>)_connection.Table<StoredEvent>()).Where(x => x.IsSnapshot && x.EventSourceId == idString)
                            .OrderByDescending(x => x.Sequence)
                            .Last()
                            .ToCommitedEvent();

        }

        public void Store(UncommittedEventStream eventStream)
        {
            _connection.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            return _connection.Table<StoredEvent>().Select(x => x.ToCommitedEvent());
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var idString = id.ToString();
            return new CommittedEventStream(id,
                                            ((TableQuery<StoredEvent>) _connection.Table<StoredEvent>())
                                                .Where(
                                                    x =>
                                                    x.EventSourceId == idString && x.Sequence >= minVersion &&
                                                    x.Sequence <= maxVersion).ToList()
                                                .Select(x => x.ToCommitedEvent()));
        }
    }
}