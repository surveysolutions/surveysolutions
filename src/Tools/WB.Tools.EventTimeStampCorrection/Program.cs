using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Replication;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Tools.EventTimeStampCorrection
{
    class Program
    {

        private static void ShowHelp()
        {
            Console.WriteLine(string.Format("Example: EventsTimeCorrection server{0}http://localhost:8080 events-db{0}DesignerEvents", delimeter));
            Console.WriteLine();
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            Dictionary<string, string> settings;
            try
            {
                settings = ParseSettings(args);
            }
            catch
            {
                Console.WriteLine("Could not parse input parameters");
                ShowHelp();
                return;
            }

            if (!requiredSettings.All(setting => settings.Keys.Contains(setting)))
            {
                ShowHelp();
                return;
            }

            Console.WriteLine("Correction started");

            try
            {
                var eventStore = CreateRavenStorage(settings[requiredSettings[0]], settings[requiredSettings[1]]);

                Console.WriteLine("Connected to db");

                int processedEventsCount = 0;
                var eventsToBeUpdated = new List<StoredEvent>();
                DateTime etalonTime = default(DateTime);

                foreach (var bulkEvents in RavenQuery<StoredEvent>(eventStore))
                {
                    foreach (var storedEvent in bulkEvents)
                    {
                        if (etalonTime != default(DateTime) && (storedEvent.EventTimeStamp < etalonTime))
                        {
                            storedEvent.EventTimeStamp = etalonTime.AddTicks(1);
                            eventsToBeUpdated.Add(storedEvent);
                        }

                        etalonTime = storedEvent.EventTimeStamp;

                        processedEventsCount++;

                        Console.Write("\r{0} event(s) processed. {1} event(s) with incorrect event timestamp",
                            processedEventsCount, eventsToBeUpdated.Count);
                    }
                }

                Console.WriteLine();

                if (eventsToBeUpdated.Any())
                {
                    
                    Console.WriteLine("Start saving to db");
                    StoreEvents(eventStore, eventsToBeUpdated);
                    Console.WriteLine("Successfully saved to db");   
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
            finally
            {
                Console.WriteLine("Correction finished");
            }
        }

        private const char delimeter = '=';
        private static readonly string[] requiredSettings = { "server", "events-db" };

        private static Dictionary<string, string> ParseSettings(string[] settings)
        {
            return settings.ToDictionary(argument => argument.Split(delimeter)[0].Trim(), argument => argument.Split(delimeter)[1].Trim());
        }

        private static DocumentStore CreateRavenStorage(string url, string db)
        {
            var store = new DocumentStore
            {
                Url = url,
                DefaultDatabase = db,
                Conventions = new DocumentConvention()
                {
                    FailoverBehavior = FailoverBehavior.FailImmediately,
                    JsonContractResolver = new PropertiesOnlyContractResolver(),
                    FindTypeTagName = x => "Events"
                }
            };

            store.Initialize();

            return store;
        }

        private static IEnumerable<T[]> RavenQuery<T>(DocumentStore documentStore)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                var query = session.Query<T, RavenDocumentsByEntityName>();

                var enumerator = session.Advanced.Stream(query);

                while (enumerator.MoveNext())
                {
                    yield return new[] { enumerator.Current.Document };
                }
            }
        }

        public static void StoreEvents(DocumentStore documentStore, IEnumerable<StoredEvent> eventsForStore)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                foreach (var eventForStore in eventsForStore)
                {
                    session.Store(eventForStore);
                }

                session.SaveChanges();
            }
        }
    }
}
