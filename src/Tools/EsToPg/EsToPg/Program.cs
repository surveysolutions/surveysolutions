using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Npgsql;
using NpgsqlTypes;

namespace EsToPg
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor(new ConsoleHost());
            processor.RegisterCommand<MigCommand>("mig");
            processor.Process(args);
        }
    }

    static class stringExtensions
    {
        // Convert the string to Pascal case.
        public static string ToPascalCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }
    }

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
        [Argument(Name = "tcpPort")]
        public int TcpPort { get; set; }

        [Description("Ip address of EventStore server.")]
        [Argument(Name = "serverIp", DefaultValue = "127.0.0.1")]
        public string ServerIp { get; set; }

        [Description("ES login.")]
        [Argument(Name = "login", DefaultValue = "admin")]
        public string Login { get; set; }

        [Description("ES password.")]
        [Argument(Name = "password", DefaultValue = "changeit")]
        public string Password { get; set; }

        [Description("Tcp port for EventStore connection.")]
        [Argument(Name = "batchSize", DefaultValue = 3000)]
        public int BatchSize { get; set; }

        [Description("PostgreSQL connection string.")]
        [Argument(Name = "pgcs")]
        public string PgConnectionString { get; set; }

        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
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
        }

        private void CreateRelations()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "EsToPg.InitEventStore.sql";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            using (var connection = new NpgsqlConnection(this.PgConnectionString))
            {
                connection.Open();

                var dbCommand = connection.CreateCommand();
                dbCommand.CommandText = reader.ReadToEnd();
                dbCommand.ExecuteNonQuery();
            }
        }

        private void SaveAllByCopy(IEnumerable<Event> events)
        {
            using (var connection = new NpgsqlConnection(PgConnectionString))
            {
                connection.Open();
                using (var writer = connection.BeginBinaryImport("COPY events(id, origin, timestamp, eventsourceid, globalsequence, value, eventsequence, eventType) FROM STDIN BINARY;"))
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

    internal class Event
    {
        public Guid Id { get; set; }
        public string Origin { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid EventSourceId { get; set; }
        public long GlobalSequence { get; set; }
        public long EventSequence { get; set; }
        public string Value { get; set; }
        public string EventType { get; set; }
    }
}
