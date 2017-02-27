using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using NConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Npgsql;
using NpgsqlTypes;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;

namespace EsToPg
{
    [Description("Migrates events from EventStore to PostgreSQL")]
    internal class MigCommand : IConsoleCommand
    {
        const string EventsPrefix = "WB-";

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        [Description("Tcp port for EventStore connection.")]
        [Argument(Name = "tcpPort", IsRequired = true)]
        public int TcpPort { get; set; }

        [Description("Ip address of EventStore server.")]
        [Argument(Name = "serverIp", IsRequired = false)]
        public string ServerIp { get; set; } = "127.0.0.1";

        [Description("ES login.")]
        [Argument(Name = "login", IsRequired = false)]
        public string Login { get; set; } = "admin";

        [Description("ES password.")]
        [Argument(Name = "password", IsRequired = false)]
        public string Password { get; set; } = "changeit";

        [Description("Tcp port for EventStore connection.")]
        [Argument(Name = "batchSize", IsRequired = false)]
        public int BatchSize { get; set; } = 3000;

        [Description("PostgreSQL connection string.")]
        [Argument(Name = "pgcs", IsRequired = true)]
        public string PgConnectionString { get; set; }

        [Description("PostgreSQL target schema name")]
        [Argument(Name = "schame", IsRequired = false)]
        public string PgSchemaName { get; set; } = "events";

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            CreateRelations();

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), TcpPort);

            var settings = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .SetDefaultUserCredentials(new UserCredentials(Login, Password));

            using (var eventStoreConnection = EventStoreConnection.Create(settings, tcpEndPoint))
            {
                Position position = Position.Start;
                AllEventsSlice slice;
                
                await eventStoreConnection.ConnectAsync();
                int writtenEvents = 0;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                do
                {
                    Stopwatch localWatch = new Stopwatch();
                    localWatch.Start();

                    slice = await eventStoreConnection.ReadAllEventsForwardAsync(position, BatchSize, false);
                    position = slice.NextPosition;


                    List<Event> events = new List<Event>();

                    foreach (var resolvedEvent in slice.Events)
                    {
                        if (resolvedEvent.Event.EventStreamId.StartsWith(EventsPrefix))
                        {
                            var metadata = JsonConvert.DeserializeObject<Event>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata), JsonSerializerSettings);
                            var value = Encoding.UTF8.GetString(resolvedEvent.Event.Data);

                            metadata.EventSequence = resolvedEvent.Event.EventNumber + 1;
                            metadata.Id = resolvedEvent.Event.EventId;
                            metadata.Value = value;
                            metadata.EventType = resolvedEvent.Event.EventType.ToPascalCase();

                            events.Add(metadata);
                        }
                    }

                    SaveAllByCopy(events);
                    writtenEvents += events.Count;

                    localWatch.Stop();
                    var charsWritten = events.Sum(x => x.Value.Length);

                    Console.WriteLine("Written {0:n0} events. {1:n0} inserted in {2:n0} ms. Chars written: {3:n0}", writtenEvents, events.Count, localWatch.ElapsedMilliseconds, charsWritten);
                } while (!slice.IsEndOfStream);

                watch.Stop();
                Console.WriteLine("Total time {0:n0} ms", watch.ElapsedMilliseconds);
            }

            return Task.FromResult(true);
        }

        private void CreateRelations()
        {
            DatabaseManagement.InitDatabase(PgConnectionString, PgSchemaName);

            var announcer = new NullAnnouncer();

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = typeof(M001_AddEventSequenceIndex).Namespace
            };

            var options = new DbMigrationsRunner.MigrationOptions
            {
                PreviewOnly = false,
            };

            var factory = new InSchemaPostgresProcessorFactory(PgSchemaName);
            using (var processor = factory.Create(PgConnectionString, announcer, options))
            {
                var runner = new MigrationRunner(typeof(M001_AddEventSequenceIndex).Assembly, migrationContext, processor);
                runner.MigrateUp();
            }
        }

        private void SaveAllByCopy(IEnumerable<Event> events)
        {
            using (var connection = new NpgsqlConnection(PgConnectionString))
            {
                connection.Open();
                using (var writer = connection.BeginBinaryImport($"COPY {PgSchemaName}.events(id, origin, timestamp, eventsourceid, globalsequence, value, eventsequence, eventType) FROM STDIN BINARY;"))
                {
                    foreach (Event @event in events)
                    {
                        writer.StartRow();
                        writer.Write(@event.Id, NpgsqlDbType.Uuid);
                        writer.Write(@event.Origin, NpgsqlDbType.Text);
                        writer.Write(@event.Timestamp, NpgsqlDbType.Timestamp);
                        writer.Write(@event.EventSourceId, NpgsqlDbType.Uuid);
                        writer.Write(@event.GlobalSequence, NpgsqlDbType.Integer);
                        writer.Write(@event.Value, NpgsqlDbType.Json);
                        writer.Write(@event.EventSequence, NpgsqlDbType.Integer);
                        writer.Write(@event.EventType, NpgsqlDbType.Text);
                    }
                }
            }
        }
    }
}