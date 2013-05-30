using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using NConfig;
using Ncqrs.Eventing.Storage.RavenDB;
using Ncqrs.Eventing.Storage.RavenDB.RavenIndexes;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace StorageAnalyzer
{
    class Program
    {
        public class EventInfo
        {
            public string Type { get; set; }
            public Guid EventSourceId { get; set; }
        }

        static void Main(string[] args)
        {
            NConfigurator.UsingFile(@"Configuration\StorageAnalyzer.config").SetAsSystemDefault();
            var url = ConfigurationManager.AppSettings["Raven.DocumentStore"];
            
            var store = (new DocumentStore { Url = url }).Initialize();

            IndexCreation.CreateIndexes(typeof (Events_EventsByDataType).Assembly, store);

            int returnedEventCount = 0;
            var allEventsWereReturned = false;
            do
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    using (IDocumentSession dsession = store.OpenSession("DjiboutiSupervisor"))
                    {
                        var storedEvent = session
                            .Query<StoredEvent>()
                            .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                            .Skip(returnedEventCount)
                            .Take(1)
                            .SingleOrDefault();

                        allEventsWereReturned = (storedEvent == null);

                        if (!allEventsWereReturned)
                        {
                            returnedEventCount++;

                            storedEvent.EventType = storedEvent.Data.GetType().Name;

                            dsession.Store(storedEvent);

                            dsession.SaveChanges();
                        }
                    }
                }
            } while (allEventsWereReturned);
        }
    }
}
