using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using NHibernate;
using Npgsql;
using NpgsqlTypes;
using WB.Infrastructure.Native.Monitoring;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    [Localizable(false)]
    public class PostgresEventStore : IHeadquartersEventStore
    {
        private readonly IEventTypeResolver eventTypeResolver;

        private string tableNameWithSchema => $"events";
        private readonly string[] obsoleteEvents = { "tabletregistered" };

        private readonly IUnitOfWork sessionProvider;
        private readonly ILogger<PostgresEventStore> logger;

        public PostgresEventStore(
            IEventTypeResolver eventTypeResolver,
            IUnitOfWork sessionProvider,
            ILogger<PostgresEventStore> logger)
        {
            this.eventTypeResolver = eventTypeResolver;
            this.sessionProvider = sessionProvider;
            this.logger = logger;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
        {
            var rawEvents = sessionProvider.Session.Connection.Query<RawEvent>(
                 $"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value::text " +
                 $"FROM {tableNameWithSchema} " +
                 $"WHERE eventsourceid= @sourceId AND eventsequence >= @minVersion " +
                 $"ORDER BY eventsequence",
                 new { sourceId = id, minVersion }, buffered: true);

            foreach (var committedEvent in ToCommittedEvent(rawEvents))
            {
                yield return committedEvent;
            }
        }

        public IEnumerable<CommittedEvent> Read(Guid aggregateRootId, params string[] typeNames)
        {
            if (typeNames.Length == 0)
                yield break;

            var rawEvents = sessionProvider.Session.Connection.Query<RawEvent>(
                 $"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value::text " +
                 $"FROM {tableNameWithSchema} " +
                 $"WHERE eventsourceid= @sourceId AND eventtype = ANY(@typeNames) " +
                 $"ORDER BY eventsequence",
                 new { sourceId = aggregateRootId, typeNames }, buffered: true);

            foreach (var committedEvent in ToCommittedEvent(rawEvents))
            {
                yield return committedEvent;
            }
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
            => this.Read(id, minVersion);

        public IEnumerable<CommittedEvent> ReadAfter(Guid aggregateRootId, Guid eventId)
        {
            var rawEvents = sessionProvider.Session.Connection.Query<RawEvent>(
                $"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value::text " +
                $"FROM {tableNameWithSchema} " +
                $"WHERE eventsourceid= @sourceId AND eventsequence > (select eventsequence from {tableNameWithSchema} where eventsourceid= @sourceId AND id = @eventId) " +
                $"ORDER BY eventsequence",
                new { sourceId = aggregateRootId, eventId }, buffered: true);

            foreach (var committedEvent in ToCommittedEvent(rawEvents))
            {
                yield return committedEvent;
            }
        }

        public int? GetLastEventSequence(Guid id)
        {
            return this.sessionProvider.Session.Connection.ExecuteScalar<int?>(
                $"SELECT MAX(eventsequence) as eventsourceid FROM {tableNameWithSchema} WHERE eventsourceid=@sourceId",
                new { sourceId = id });
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                return new CommittedEventStream(eventStream.SourceId, this.Store(eventStream, this.sessionProvider.Session));
            }

            return new CommittedEventStream(eventStream.SourceId);
        }

        private List<CommittedEvent> Store(UncommittedEventStream eventStream, ISession connection)
        {
            var result = new List<CommittedEvent>();

            ValidateStreamVersion(eventStream, connection);

            var copyFromCommand = $"COPY {tableNameWithSchema}(id, origin, timestamp, eventsourceid, value, eventsequence, eventtype) FROM STDIN BINARY;";
            var npgsqlConnection = (NpgsqlConnection)connection.Connection;

            // fix for KP-13687
            npgsqlConnection.Execute($"lock table {tableNameWithSchema} in ROW EXCLUSIVE mode");

            using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
            {
                foreach (var @event in eventStream)
                {
                    var eventString = JsonConvert.SerializeObject(@event.Payload, Formatting.Indented,
                        EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

                    writer.StartRow();
                    writer.Write(@event.EventIdentifier, NpgsqlDbType.Uuid);
                    writer.Write(@event.Origin, NpgsqlDbType.Text);
                    writer.Write(@event.EventTimeStamp, NpgsqlDbType.Timestamp);
                    writer.Write(@event.EventSourceId, NpgsqlDbType.Uuid);
                    writer.Write(eventString, NpgsqlDbType.Jsonb);
                    writer.Write(@event.EventSequence, NpgsqlDbType.Integer);
                    writer.Write(@event.Payload.GetType().Name, NpgsqlDbType.Text);

                    var committedEvent = new CommittedEvent(eventStream.CommitId,
                        @event.Origin,
                        @event.EventIdentifier,
                        @event.EventSourceId,
                        @event.EventSequence,
                        @event.EventTimeStamp,
                        null,
                        @event.Payload);
                    result.Add(committedEvent);
                }

                writer.Complete();
                CommonMetrics.EventsCreatedCount.Inc(result.Count);
            }

            return result;
        }

        private void ValidateStreamVersion(UncommittedEventStream eventStream, ISession connection)
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
                    validateVersionCommand.CommandText = $"SELECT EXISTS(SELECT 1 FROM {tableNameWithSchema} WHERE eventsourceid = :sourceId)";
                    AppendEventSourceParameter(validateVersionCommand);

                    var streamExists = validateVersionCommand.ExecuteScalar() as bool?;
                    if (streamExists.GetValueOrDefault())
                        throw new InvalidOperationException(
                            $"Unexpected stream version. Expected non existant stream, but received stream with version {eventStream.InitialVersion}. EventSourceId: {eventStream.SourceId}");
                }
            }
            else
            {
                var eventsFromDb = connection.Connection.Query<int>($"SELECT eventsequence FROM {tableNameWithSchema} " +
                    $"WHERE eventsourceid = :sourceId AND (eventsequence=:sequence OR eventsequence=:sequence + 1) ORDER BY eventsequence",
                    new
                    {
                        sourceId = eventStream.SourceId,
                        sequence = eventStream.InitialVersion
                    }).ToList();
                if (eventsFromDb.Count != 1 || eventsFromDb[0] != eventStream.InitialVersion)
                {
                    throw new InvalidOperationException(
                        $"Unexpected stream version. Expected {eventStream.InitialVersion}. EventSourceId: {eventStream.SourceId}");
                }
            }
        }

        public bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId, params string[] typeNames)
        {
            var connection = sessionProvider.Session.Connection as NpgsqlConnection;
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

        public int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames)
        {
            return this.sessionProvider.Session.Connection.ExecuteScalar<int?>(
                $@"select MAX(eventsequence) from {tableNameWithSchema} where
                                        eventsourceid = @eventSourceId
                                        and eventtype = ANY(@eventTypes)
                                        limit 1", new { eventSourceId, eventTypes = typeNames });
        }

        public Guid? GetLastEventId(Guid eventSourceId, params string[] excludeTypeNames)
        {
            return this.sessionProvider.Session.Connection.ExecuteScalar<Guid?>(
                $@"select id from {tableNameWithSchema} where
                                        eventsourceid = @eventSourceId
                                        and NOT (eventtype = ANY(@eventTypes))
                                        order by eventsequence desc
                                        limit 1",
                new { eventSourceId, eventTypes = excludeTypeNames }
                );
        }

        public IEnumerable<RawEvent> GetRawEventsFeed(long startWithGlobalSequence, int pageSize, long limitSize = long.MaxValue)
        {
            // fix for KP-13687
            var sessionConnection = this.sessionProvider.Session.Connection;
            sessionConnection.Execute($"lock table {tableNameWithSchema} in SHARE mode");

            var rawEventsData = sessionConnection
                .Query<RawEvent>
                ($@"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value::text 
                   FROM {tableNameWithSchema} 
                   WHERE globalsequence > @minVersion 
                   ORDER BY globalsequence 
                   LIMIT @batchSize",
                    new { minVersion = startWithGlobalSequence, batchSize = pageSize }, buffered: false);

            var enumerator = rawEventsData.GetEnumerator();
            long limit = limitSize;
            long eventsRead = 0;

            try
            {
                while (enumerator.MoveNext())
                {
                    var ev = enumerator.Current;

                    yield return ev;
                    eventsRead++;

                    limit -= ev.Value.Length;

                    if (limit <= 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                try
                {
                    enumerator.Dispose();
                }
                catch (PostgresException ne) when (ne.SqlState == "57014")
                {
                    // 57014: canceling statement due to user request 
                    // thrown by breaking IEnumerable enumeration.          
                    this.logger.LogWarning($"Cancelling events reading due to set read limit: {limitSize} bytes. " +
                        $"Read {eventsRead} instead of requested page size of: {pageSize}");
                }
            }
        }

        public async Task<List<CommittedEvent>> GetEventsInReverseOrderAsync(Guid aggregateRootId, int offset, int limit)
        {
            List<CommittedEvent> result = new List<CommittedEvent>();
            var connection = sessionProvider.Session.Connection;
            var rawEvents = await connection.QueryAsync<RawEvent>(
                        $"SELECT id, eventsourceid, origin, eventsequence, timestamp, globalsequence, eventtype, value::text " +
                        $"FROM {tableNameWithSchema} " +
                        "WHERE eventsourceid = @sourceId " +
                        "ORDER BY eventsequence DESC LIMIT @limit OFFSET @offset",
                        new
                        {
                            sourceId = aggregateRootId,
                            limit = limit,
                            offset = offset
                        });

            foreach (var committedEvent in ToCommittedEvent(rawEvents))
            {
                result.Add(committedEvent);
            }

            return result;
        }

        public async Task<int> TotalEventsCountAsync(Guid aggregateRootId)
        {
            var connection = sessionProvider.Session.Connection;
            var result = await connection.ExecuteScalarAsync<int?>(
                    $"SELECT COUNT(id) FROM {tableNameWithSchema} WHERE eventsourceid=:sourceId",
                    new
                    {
                        sourceId = aggregateRootId
                    });

            return result.GetValueOrDefault();
        }

        public async Task<long> GetMaximumGlobalSequence()
        {
            return await this.sessionProvider.Session.Connection.ExecuteScalarAsync<long?>(
                "SELECT max(globalsequence) FROM events") ?? 0;
        }

        private IEnumerable<CommittedEvent> ToCommittedEvent(IEnumerable<RawEvent> rawEventsData)
        {
            // reusing serializer to save few bytes of allocation
            var serializer = JsonSerializer.CreateDefault(EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

            IEvent Deserialize(string value, Type type)
            {
                using JsonTextReader reader = new JsonTextReader(new StringReader(value));
                return (IEvent)serializer.Deserialize(reader, type);
            }

            foreach (var raw in rawEventsData.AsParallel())
            {
                if (obsoleteEvents.Contains(raw.EventType.ToLower()))
                {
                    continue;
                }

                var resolvedEventType = this.eventTypeResolver.ResolveType(raw.EventType);
                IEvent typedEvent = Deserialize(raw.Value, resolvedEventType);

                yield return new CommittedEvent(
                    Guid.Empty,
                    raw.Origin,
                    raw.Id,
                    raw.EventSourceId,
                    raw.EventSequence,
                    raw.TimeStamp,
                    raw.GlobalSequence,
                    typedEvent
                );
            }
        }

        public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
        {
            return this.sessionProvider.Session.Connection.ExecuteScalar<int?>(
                $"SELECT 1 FROM {tableNameWithSchema} WHERE eventsourceid=@sourceId AND eventsequence=@next",
                new
                {
                    sourceId = eventSourceId,
                    next = lastKnownEventSequence + 1
                }) == 1;
        }
    }
}
