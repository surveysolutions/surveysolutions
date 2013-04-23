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
       // private readonly ISQLiteConnection _connection;
        public MvvmCrossSqliteEventStore()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            _connectionFactory = this.GetService<ISQLiteConnectionFactory>();

            //  _connection = connectionFactory.Create("EventStore");
            WrapConnection((c) => c.CreateTable<StoredEvent>());

        }

        private readonly  ISQLiteConnectionFactory _connectionFactory;


        public Ncqrs.Eventing.CommittedEvent GetLatestSnapshoot(Guid id)
        {
            return
                WrapConnection<CommittedEvent>(
                    (c) => c.Table<StoredEvent>().Where(x => x.IsSnapshot && x.EventSourceId == id.ToString())
                            .OrderByDescending(x => x.Sequence)
                            .Last()
                            .ToCommitedEvent());

        }

        public void Store(UncommittedEventStream eventStream)
        {
            WrapConnection((c) => c.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true));
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            return
                WrapConnection<IEnumerable<CommittedEvent>>(
                    (c) => c.Table<StoredEvent>().Select(x => x.ToCommitedEvent()));
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            return
                WrapConnection<CommittedEventStream>(
                    (c) => new CommittedEventStream(id,
                                                    c.Table<StoredEvent>()
                                                     .Where(
                                                         x =>
                                                         x.EventSourceId == id.ToString() && x.Sequence >= minVersion &&
                                                         x.Sequence <= maxVersion)
                                                     .Select(x => x.ToCommitedEvent())));
        }

       /* private void WrapConnection(Action<ISQLiteConnection> action)
        {
            using (var connection = _connectionFactory.Create("EventStore"))
            {
                action(connection);
            }
        }*/

        private T WrapConnection<T>(Func<ISQLiteConnection, T> action)
        {
            using (var connection = _connectionFactory.Create("EventStore"))
            {
                return action(connection);
            }
        }
    }
}