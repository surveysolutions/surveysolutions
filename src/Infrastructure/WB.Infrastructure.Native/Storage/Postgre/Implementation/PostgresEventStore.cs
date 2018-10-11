using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using NHibernate;
using Npgsql;
using NpgsqlTypes;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    [Localizable(false)]
    public class PostgresEventStore  : IHeadquartersEventStore
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private static long lastUsedGlobalSequence = -1;
        private static readonly object lockObject = new object();
        private readonly IEventTypeResolver eventTypeResolver;
        private readonly ISessionProvider sessionProvider;
        private static int BatchSize = 4096;
        private static string tableNameWithSchema;
        private readonly string tableName;
        private readonly string[] obsoleteEvents = new[] { "tabletregistered" };

        public PostgresEventStore(PostgreConnectionSettings connectionSettings, 
            IEventTypeResolver eventTypeResolver,
            ISessionProvider sessionProvider)
        {
            this.connectionSettings = connectionSettings;
            this.eventTypeResolver = eventTypeResolver;
            this.sessionProvider = sessionProvider;

            this.tableName = "events";
            tableNameWithSchema = connectionSettings.SchemaName + "." + this.tableName;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
        {
            int processed = 0;
            IEnumerable<CommittedEvent> batch;
            do
            {
                batch = this.ReadBatch(id, minVersion, processed).ToList();
                foreach (var @event in batch)
                {
                    processed++;
                    yield return @event;
                }
            } while (batch.Any());
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
            => this.Read(id, minVersion);

        private IEnumerable<CommittedEvent> ReadBatch(Guid id, int minVersion, int processed)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM {tableNameWithSchema} WHERE eventsourceid=:sourceId AND eventsequence >= :minVersion ORDER BY eventsequence LIMIT :batchSize OFFSET :processed";
                    command.Parameters.AddWithValue("sourceId", NpgsqlDbType.Uuid, id);
                    command.Parameters.AddWithValue("minVersion", minVersion);
                    command.Parameters.AddWithValue("batchSize", BatchSize);
                    command.Parameters.AddWithValue("processed", processed);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var singleEvent = this.ReadSingleEvent(reader);
                            if (singleEvent != null)
                                yield return singleEvent;
                        }
                    }
                }
            }
        }

        public int? GetLastEventSequence(Guid id)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT MAX(eventsequence) as eventsourceid FROM {tableNameWithSchema} WHERE eventsourceid=:sourceId";
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
                return new CommittedEventStream(eventStream.SourceId, this.Store(eventStream, this.sessionProvider.GetSession()));
            }

            return new CommittedEventStream(eventStream.SourceId);
        }

        private List<CommittedEvent> Store(UncommittedEventStream eventStream, ISession connection)
        {
            var result = new List<CommittedEvent>();

            ValidateStreamVersion(eventStream, connection);

            var copyFromCommand =
                $"COPY {tableNameWithSchema}(id, origin, timestamp, eventsourceid, globalsequence, value, eventsequence, eventtype) FROM STDIN BINARY;";
            var npgsqlConnection = connection.Connection as NpgsqlConnection;

            using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
            {
                foreach (var @event in eventStream)
                {
                    var eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented,
                        EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);
                    var nextSequence = this.GetNextSequence();

                    writer.StartRow();
                    writer.Write(@event.EventIdentifier, NpgsqlDbType.Uuid);
                    writer.Write(@event.Origin, NpgsqlDbType.Text);
                    writer.Write(@event.EventTimeStamp, NpgsqlDbType.Timestamp);
                    writer.Write(@event.EventSourceId, NpgsqlDbType.Uuid);
                    writer.Write(nextSequence, NpgsqlDbType.Integer);
                    writer.Write(eventString, NpgsqlDbType.Jsonb);
                    writer.Write(@event.EventSequence, NpgsqlDbType.Integer);
                    writer.Write(@event.Payload.GetType().Name, NpgsqlDbType.Text);

                    var committedEvent = new CommittedEvent(eventStream.CommitId,
                        @event.Origin,
                        @event.EventIdentifier,
                        @event.EventSourceId,
                        @event.EventSequence,
                        @event.EventTimeStamp,
                        nextSequence,
                        @event.Payload);
                    result.Add(committedEvent);
                }
            }

            return result;
        }

        private static void ValidateStreamVersion(UncommittedEventStream eventStream, ISession connection)
        {
            void AppendEventSourceParameter(IDbCommand command)
            {
                IDbDataParameter sourceIdParameter = command.CreateParameter();
                sourceIdParameter.Value = eventStream.SourceId;
                sourceIdParameter.DbType = DbType.Guid;
                sourceIdParameter.ParameterName = "sourceId";
                command.Parameters.Add(sourceIdParameter);
            }

            if (eventStream.InitialVersion == 0)
            {
                using (var validateVersionCommand = connection.Connection.CreateCommand())
                {
                    validateVersionCommand.CommandText =
                        $"SELECT EXISTS(SELECT 1 FROM {tableNameWithSchema} WHERE eventsourceid = :sourceId)";
                    AppendEventSourceParameter(validateVersionCommand);

                    var streamExists = validateVersionCommand.ExecuteScalar() as bool?;
                    if (streamExists.GetValueOrDefault())
                        throw new InvalidOperationException(
                            $"Unexpected stream version. Expected non existant stream, but received stream with version {eventStream.InitialVersion}. EventSourceId: {eventStream.SourceId}");
                }
            }
            else
            {
                using (var validateVersionCommand = connection.Connection.CreateCommand())
                {
                    validateVersionCommand.CommandText =
                        $"SELECT MAX(eventsequence) FROM {tableNameWithSchema} WHERE eventsourceid = :sourceId";
                    AppendEventSourceParameter(validateVersionCommand);

                    var storedLastSequence = validateVersionCommand.ExecuteScalar() as int?;
                    if (storedLastSequence != eventStream.InitialVersion)
                        throw new InvalidOperationException(
                            $"Unexpected stream version. Expected {eventStream.InitialVersion}. Actual {storedLastSequence}. EventSourceId: {eventStream.SourceId}");
                }
            }
        }

        public int CountOfAllEvents()
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select reltuples::bigint from pg_class where relname='{this.tableName}'";
                var scalar = command.ExecuteScalar();

                var result = scalar == null ? 0 : Convert.ToInt32(scalar);
                if (result == 0)
                {
                    var countCommand = connection.CreateCommand();
                    countCommand.CommandText = $"select count(id) from {tableNameWithSchema}";
                    var exactCountScalar = countCommand.ExecuteScalar();
                    result = exactCountScalar == null ? 0 : Convert.ToInt32(exactCountScalar);
                }
                return result;
            }
        }

        public bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId, params string[] typeNames)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $@"select 1 from {tableNameWithSchema} where
                                        eventsourceid = :eventSourceId
                                        and eventsequence > :eventSequence
                                        and eventtype = ANY(:eventTypes)
                                        limit 1";
                command.Parameters.AddWithValue("eventSourceId", NpgsqlDbType.Uuid, eventSourceId);
                command.Parameters.AddWithValue("eventSequence", NpgsqlDbType.Bigint, sequence);
                command.Parameters.AddWithValue("eventTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, typeNames);
                var scalar = command.ExecuteScalar();
                return scalar != null;
            }
        }

        public int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames)
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $@"select MAX(eventsequence) from {tableNameWithSchema} where
                                        eventsourceid = :eventSourceId
                                        and eventtype = ANY(:eventTypes)
                                        limit 1";
                command.Parameters.AddWithValue("eventSourceId", NpgsqlDbType.Uuid, eventSourceId);
                command.Parameters.AddWithValue("eventTypes", NpgsqlDbType.Array | NpgsqlDbType.Text, typeNames);
                var scalar = command.ExecuteScalar();
                return scalar == DBNull.Value ? null : (int?) scalar;
            }
        }

        public async Task<EventsFeedPage> GetEventsFeedAsync(long startWithGlobalSequence, int pageSize)
        {
            List<CommittedEvent> events = new List<CommittedEvent>();

            var npgConnection = this.sessionProvider.GetSession().Connection as NpgsqlConnection;

            var command = npgConnection.CreateCommand();
            command.CommandText =
                $@"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value FROM {tableNameWithSchema} 
                                         WHERE globalsequence > @minVersion 
                                         ORDER BY globalsequence 
                                         LIMIT @batchSize";
            command.Parameters.AddWithValue("minVersion", startWithGlobalSequence);
            command.Parameters.AddWithValue("batchSize", pageSize);

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var singleEvent = this.ReadSingleEvent(reader);
                    if (singleEvent != null)
                        events.Add(singleEvent);
                }
            }

            return new EventsFeedPage(GetLastGlobalSequence(), events);
        }

        private CommittedEvent ReadSingleEvent(IDataReader dataReader)
        {
            string value = (string) dataReader["value"];

            string eventType = (string) dataReader["eventtype"];

            if (obsoleteEvents.Contains(eventType.ToLower()))
                return null;

            var resolvedEventType = this.eventTypeResolver.ResolveType(eventType);
            IEvent typedEvent = JsonConvert.DeserializeObject(value, resolvedEventType, EventSerializerSettings.BackwardCompatibleJsonSerializerSettings) as IEvent;

            var origin = dataReader["origin"];

            var eventIdentifier = (Guid) dataReader["id"];
            var eventSourceId = (Guid) dataReader["eventsourceid"];
            var eventSequence = (int) dataReader["eventsequence"];
            var eventTimeStamp = (DateTime) dataReader["timestamp"];
            var globalSequence = (int) dataReader["globalsequence"];

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

        private long GetNextSequence()
        {
            GetLastGlobalSequence();

            return Interlocked.Increment(ref lastUsedGlobalSequence);
        }

        private long GetLastGlobalSequence()
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

            return lastUsedGlobalSequence;
        }

        private void FillLastUsedSequenceInEventStore()
        {
            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select MAX(globalsequence) from {tableNameWithSchema}";

                var scalar = command.ExecuteScalar();
                lastUsedGlobalSequence = scalar is DBNull ? 0 : Convert.ToInt32(scalar);
            }
        }
    }
}
