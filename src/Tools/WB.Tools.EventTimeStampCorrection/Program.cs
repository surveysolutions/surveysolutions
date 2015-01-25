using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Replication;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide.Indexes;

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
                if (eventStore.DatabaseCommands.GetIndex("Event_ByEventSource") == null)
                {
                    eventStore.ExecuteIndex(new Event_ByEventSource());
                }

                Console.WriteLine("Connected to db");

                long maxEventSequence = GetMaxEventSequenceNo(eventStore);

                int processedEventsCount = 0;
                DateTime etalonTime = default(DateTime);

                var eventsToBeUpdated = new List<StoredEvent>();
                var timeStampsByEventSource = new Dictionary<Guid, DateTime>();

                for (int currentEventSequence = 1; currentEventSequence <= maxEventSequence; currentEventSequence++)
                {
                    foreach (var bulkEvents in RavenQuery(eventStore, currentEventSequence))
                    {
                        foreach (var storedEvent in bulkEvents)
                        {
                            if (currentEventSequence == 1)
                            {
                                if (etalonTime == default(DateTime))
                                {
                                    etalonTime = storedEvent.EventTimeStamp;
                                }
                                else
                                {
                                    if (storedEvent.EventTimeStamp < etalonTime)
                                    {
                                        storedEvent.EventTimeStamp = etalonTime.AddTicks(1);
                                        eventsToBeUpdated.Add(storedEvent);
                                    }

                                    etalonTime = storedEvent.EventTimeStamp;   
                                }
                                timeStampsByEventSource.Add(storedEvent.EventSourceId, storedEvent.EventTimeStamp);
                            }
                            else
                            {
                                if (storedEvent.EventTimeStamp < timeStampsByEventSource[storedEvent.EventSourceId])
                                {
                                    storedEvent.EventTimeStamp = timeStampsByEventSource[storedEvent.EventSourceId].AddTicks(1);
                                    eventsToBeUpdated.Add(storedEvent);

                                    timeStampsByEventSource[storedEvent.EventSourceId] = storedEvent.EventTimeStamp;
                                }
                            }

                            processedEventsCount++;

                            Console.Write("\r{0} event(s) processed. {1} event(s) with incorrect event timestamp",
                                processedEventsCount, eventsToBeUpdated.Count);
                        }
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

        private static IEnumerable<StoredEvent[]> RavenQuery(DocumentStore documentStore, long sequenceNo)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                var query = session.Query<StoredEvent, Event_ByEventSource>().Where(evt=>evt.EventSequence == sequenceNo);

                var enumerator = session.Advanced.Stream(query);

                while (enumerator.MoveNext())
                {
                    yield return new[] { enumerator.Current.Document };
                }
            }
        }

        private static long GetMaxEventSequenceNo(DocumentStore documentStore)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<StoredEvent, Event_ByEventSource>().OrderByDescending(x => x.EventSequence).Take(1).ToArray()[0].EventSequence;
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
