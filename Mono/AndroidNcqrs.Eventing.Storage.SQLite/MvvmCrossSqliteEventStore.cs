using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
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

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IStreamableEventStore, ISnapshootEventStore, IMvxServiceConsumer
    {
        private readonly ISQLiteConnection _connection;
        public MvvmCrossSqliteEventStore()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();

            _connection = connectionFactory.Create("EventStore");
            Connection.CreateTable<StoredEvent>();
        }

   //     private readonly  ISQLiteConnectionFactory _connectionFactory;


        protected ISQLiteConnection Connection
        {
            get { return _connection; }
        }

        public Ncqrs.Eventing.CommittedEvent GetLatestSnapshoot(Guid id)
        {
            return
                Connection.Table<StoredEvent>().Where(x => x.IsSnapshot)
                           .OrderByDescending(x => x.Sequence)
                           .Last()
                           .ToCommitedEvent();
        }

        public void Store(UncommittedEventStream eventStream)
        {
            Connection.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            return
                Connection.Table<StoredEvent>().Select(x => x.ToCommitedEvent());
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            return new CommittedEventStream(id,
                                            Connection.Table<StoredEvent>()
                                                       .Where(x => x.EventSourceId == id.ToString())
                                                       .Select(x => x.ToCommitedEvent()));
        }
    }
}