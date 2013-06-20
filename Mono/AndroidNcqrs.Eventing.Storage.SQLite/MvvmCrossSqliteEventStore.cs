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

using SQLite;
using SQLiteException = Android.Database.Sqlite.SQLiteException;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class MvvmCrossSqliteEventStore : IStreamableEventStore, IMvxServiceConsumer
    {
        private readonly ISQLiteConnection _connection;

        public MvvmCrossSqliteEventStore(string databaseName)
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(databaseName);
            _connection.CreateTable<StoredEvent>();

        }


        public void Store(UncommittedEventStream eventStream)
        {
            _connection.InsertAll(eventStream.Select(x => x.ToStoredEvent()), true);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            return _connection.Table<StoredEvent>().Select(x => x.ToCommitedEvent());
        }

        public Guid? GetLastEvent(Guid aggregateRootId)
        {
            var idString = aggregateRootId.ToString();
            try
            {
                var eventIDIsString=
                    ((TableQuery<StoredEvent>) _connection.Table<StoredEvent>()).Where(x => x.EventSourceId == idString)
                                                                                .OrderByDescending(x => x.Sequence)
                                                                                .Select(s =>
                                                                                        new StoredEventWithoutPayload()
                                                                                            {
                                                                                                EventId = s.EventId,
                                                                                                EventSourceId = idString,
                                                                                                Sequence = s.Sequence
                                                                                            })
                                                                                .First().EventId;
                return Guid.Parse(eventIDIsString);
            }
            catch
            {
                return null;
            }
        }

        public bool IsEventPresent(Guid aggregateRootId, Guid eventIdentifier)
        {
            var eventIdentifierString = eventIdentifier.ToString();
            try
            {
                var lastEvent = _connection.Get<StoredEvent>(eventIdentifierString);
                if (lastEvent != null && lastEvent.EventSourceId == aggregateRootId.ToString())
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
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

        public CommittedEventStream ReadFromWithoutPayload(Guid id, long minVersion, long maxVersion)
        {
            var idString = id.ToString();
            return new CommittedEventStream(id,
                                            ((TableQuery<StoredEvent>) _connection.Table<StoredEvent>())
                                                .Where(
                                                    x =>
                                                    x.EventSourceId == idString && x.Sequence >= minVersion &&
                                                    x.Sequence <= maxVersion)
                                                .Select(
                                                    s =>
                                                    new StoredEventWithoutPayload()
                                                        {
                                                            EventId = s.EventId,
                                                            EventSourceId = idString,
                                                            Sequence = s.Sequence
                                                        }).ToList()
                                                .Select(s => s.ToCommitedEventWithoutPayload()));

        }

        public int CountOfAllEventsWithoutSnapshots()
        {
            throw new NotImplementedException("Not needed for Mono so far.");
        }

        public int CountOfAllEventsIncludingSnapshots()
        {
            throw new NotImplementedException("Not needed for Mono so far.");
        }

        public IEnumerable<CommittedEvent[]> GetAllEventsWithoutSnapshots(int bulkSize)
        {
            throw new NotImplementedException("Not needed for Mono so far.");
        }

        public IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize)
        {
            throw new NotImplementedException("Not needed for Mono so far.");
        }

        public void CleanStream(Guid id)
        {
            _connection.Execute("delete from StoredEvent where EventSourceId = ?", id.ToString());
        }
    }
}