using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public class PostgresEventStore : IStreamableEventStore
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private static long lastUsedGlobalSequence = -1;
        private static readonly object lockObject = new object();
        private readonly IEventTypeResolver eventTypeResolver;
        private static int BatchSize = 4096;
        private static string tableName;
        private readonly string rawTableName;

        public PostgresEventStore(PostgreConnectionSettings connectionSettings, 
            IEventTypeResolver eventTypeResolver)
        {
            this.connectionSettings = connectionSettings;
            this.eventTypeResolver = eventTypeResolver;

            rawTableName = "events";
            tableName = connectionSettings.SchemaName + "." + this.rawTableName;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM {tableName} WHERE eventsourceid=:sourceId AND eventsequence >= {minVersion} ORDER BY eventsequence";
                    command.Parameters.AddWithValue("sourceId", NpgsqlDbType.Uuid, id);

                    using (IDataReader npgsqlDataReader = command.ExecuteReader())
                    {
                        while (npgsqlDataReader.Read())
                        {
                            var commitedEvent = this.ReadSingleEvent(npgsqlDataReader);

                            yield return commitedEvent;
                        }
                    }
                }
            }
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
            => this.Read(id, minVersion);

        public int? GetLastEventSequence(Guid id)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT MAX(eventsequence) as eventsourceid FROM {tableName} WHERE eventsourceid=:sourceId";
                    command.Parameters.AddWithValue("sourceId", NpgsqlDbType.Uuid, id);
                    var executeScalar = command.ExecuteScalar() as int?;
                    return executeScalar;
                }
            }
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
                {
                    connection.Open();
                    return new CommittedEventStream(eventStream.SourceId, this.Store(eventStream, connection));
                }
            }

            return new CommittedEventStream(eventStream.SourceId);
        }

        private List<CommittedEvent> Store(UncommittedEventStream eventStream, NpgsqlConnection connection)
        {
            var result = new List<CommittedEvent>();
            using (var npgsqlTransaction = connection.BeginTransaction())
            {
                var copyFromCommand = $"COPY {tableName}(id, origin, timestamp, eventsourceid, globalsequence, value, eventsequence, eventtype) FROM STDIN BINARY;";
                using (var writer = connection.BeginBinaryImport(copyFromCommand))
                {
                    foreach (var @event in eventStream)
                    {
                        var eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented,
                            EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
                        var nextSequnce = this.GetNextSequnce();

                        writer.StartRow();
                        writer.Write(@event.EventIdentifier, NpgsqlDbType.Uuid);
                        writer.Write(@event.Origin, NpgsqlDbType.Text);
                        writer.Write(@event.EventTimeStamp, NpgsqlDbType.Timestamp);
                        writer.Write(@event.EventSourceId, NpgsqlDbType.Uuid);
                        writer.Write(nextSequnce, NpgsqlDbType.Integer);
                        writer.Write(eventString, NpgsqlDbType.Json);
                        writer.Write(@event.EventSequence, NpgsqlDbType.Integer);
                        writer.Write(@event.Payload.GetType().Name, NpgsqlDbType.Text);

                        var committedEvent = new CommittedEvent(eventStream.CommitId,
                            @event.Origin,
                            @event.EventIdentifier,
                            @event.EventSourceId,
                            @event.EventSequence,
                            @event.EventTimeStamp,
                            nextSequnce,
                            @event.Payload);
                        result.Add(committedEvent);
                    }
                }

                npgsqlTransaction.Commit();
            }
            return result;
        }

        public int CountOfAllEvents()
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select reltuples::bigint from pg_class where relname='{this.rawTableName}'";
                var scalar = command.ExecuteScalar();

                return scalar == null ? 0 : Convert.ToInt32(scalar);
            }
        }

        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            int processed = 0;
            IEnumerable<CommittedEvent> eventsBatch;
            do
            {
                eventsBatch = this.ReadEventsBatch(processed).ToList();
                foreach (var committedEvent in eventsBatch)
                {
                    processed++;
                    yield return committedEvent;
                }
            } while (eventsBatch.Any());
        }

        private IEnumerable<CommittedEvent> ReadEventsBatch(int processed)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                conn.Open();

                var npgsqlCommand = conn.CreateCommand();
                npgsqlCommand.CommandText = $"SELECT * FROM {tableName} ORDER BY globalsequence LIMIT {BatchSize} OFFSET {processed}";

                using (var reader = npgsqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return this.ReadSingleEvent(reader);
                    }
                }
            }
        }

        public IEnumerable<EventSlice> GetEventsAfterPosition(EventPosition? position)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();

                int globalSequence = 0;
                if (position.HasValue && position.Value.EventSourceIdOfLastEvent != Guid.Empty)
                {
                    var lastGlobalSequenceCommand = connection.CreateCommand();
                    lastGlobalSequenceCommand.CommandText = $"SELECT globalsequence FROM {tableName} WHERE eventsourceid=:eventSourceId AND eventsequence = :sequence";
                    lastGlobalSequenceCommand.Parameters.AddWithValue("eventSourceId", position.Value.EventSourceIdOfLastEvent);
                    lastGlobalSequenceCommand.Parameters.AddWithValue("sequence", position.Value.SequenceOfLastEvent);
                    globalSequence = (int)lastGlobalSequenceCommand.ExecuteScalar();
                }

                long eventsCountAfterPosition = this.GetEventsCountAfterPosition(position);
                long processed = 0;
                while (processed < eventsCountAfterPosition)
                {
                    var npgsqlCommand = connection.CreateCommand();
                    npgsqlCommand.CommandText = $"SELECT * FROM {tableName} WHERE globalsequence > {globalSequence} ORDER BY globalsequence LIMIT {BatchSize} OFFSET {processed}";

                    List<CommittedEvent> events = new List<CommittedEvent>();

                    using (var reader = npgsqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(this.ReadSingleEvent(reader));
                        }
                    }

                    var committedEvent = events.Last();
                    yield return new EventSlice(events, new EventPosition(0, 0, committedEvent.EventSourceId, committedEvent.EventSequence), false);

                    processed += BatchSize;
                }

            }
        }

        public long GetEventsCountAfterPosition(EventPosition? position)
        {
            if (!position.HasValue || position.Value.EventSourceIdOfLastEvent == Guid.Empty)
                return this.CountOfAllEvents();

            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();

                var npgsqlCommand = connection.CreateCommand();
                npgsqlCommand.CommandText = $"SELECT globalsequence FROM {tableName} WHERE eventsourceid=:eventSourceId AND eventsequence = :sequence";
                var positionValue = position.Value;
                npgsqlCommand.Parameters.AddWithValue("eventSourceId", positionValue.EventSourceIdOfLastEvent);
                npgsqlCommand.Parameters.AddWithValue("sequence", positionValue.SequenceOfLastEvent);

                int globalSequence = (int) npgsqlCommand.ExecuteScalar();

                NpgsqlCommand countCommand = connection.CreateCommand();
                countCommand.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE globalsequence > {globalSequence}";
                countCommand.Parameters.AddWithValue("globalSequence", globalSequence);

                return (long) countCommand.ExecuteScalar();
            }
        }

        private CommittedEvent ReadSingleEvent(IDataReader npgsqlDataReader)
        {
            string value = (string) npgsqlDataReader["value"];

            string eventType = (string) npgsqlDataReader["eventtype"];
            var resolvedEventType = this.eventTypeResolver.ResolveType(eventType);
            IEvent typedEvent = JsonConvert.DeserializeObject(value, resolvedEventType, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings) as IEvent;

            var origin = npgsqlDataReader["origin"];

            var eventIdentifier = (Guid) npgsqlDataReader["id"];
            var eventSourceId = (Guid) npgsqlDataReader["eventsourceid"];
            var eventSequence = (int) npgsqlDataReader["eventsequence"];
            var eventTimeStamp = (DateTime) npgsqlDataReader["timestamp"];
            var globalSequence = (int) npgsqlDataReader["globalsequence"];

            var commitedEvent = new CommittedEvent(Guid.Empty,
                origin is DBNull ? null : (string) origin,
                eventIdentifier,
                eventSourceId,
                eventSequence,
                eventTimeStamp,
                globalSequence,
                typedEvent
                );
            return commitedEvent;
        }

        private long GetNextSequnce()
        {
            if (lastUsedGlobalSequence == -1)
            {
                lock (lockObject)
                {
                    if (lastUsedGlobalSequence == -1)
                    {
                        this.FillLastUsedSequenceInEventStore();
                    }
                }
            }

            Interlocked.Increment(ref lastUsedGlobalSequence);
            return lastUsedGlobalSequence;
        }

        private void FillLastUsedSequenceInEventStore()
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select MAX(globalsequence) from {tableName}";

                var scalar = command.ExecuteScalar();
                lastUsedGlobalSequence = scalar is DBNull ? 0 : Convert.ToInt32(scalar);
            }
        }
    }
}