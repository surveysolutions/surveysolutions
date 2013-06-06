using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using NConfig;
using Ncqrs.Eventing.Storage.RavenDB;
using Ncqrs.Eventing.Storage.RavenDB.RavenIndexes;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace StorageAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            NConfigurator.UsingFile(@"Configuration\StorageAnalyzer.config").SetAsSystemDefault();
            var url = ConfigurationManager.AppSettings["Raven.DocumentStore"];
            
            var store = (new DocumentStore { Url = url }).Initialize();

            //IndexCreation.CreateIndexes(typeof (Events_EventsByDataType).Assembly, store);

            //store.DatabaseCommands.ForDatabase("DjiboutiSupervisor")
            //     .PutIndex("Events_EventsByDataType", new IndexDefinition()
            //         {
            //             Map = "from sEvent in docs.Events select new { Type = sEvent.EventType, Count = 1 }",
            //             Reduce = "from result in results group result by result.Type into g select new { Type = g.Key, Count = g.Sum(x => x.Count) }"
            //         }, true);

            int returnedEventCount = 0;
            var allEventsWereReturned = false;
            do
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    using (IDocumentSession dsession = store.OpenSession("DjiboutiSupervisor"))
                    {
                        var storedEvent = session
                            .Advanced.LuceneQuery<StoredEvent>("Raven/DocumentsByEntityName")
                            .Where("Tag:Events")
                            .Skip(returnedEventCount)
                            .Take(1)
                            .SingleOrDefault();

                        allEventsWereReturned = (storedEvent == null);

                        if (!allEventsWereReturned)
                        {
                            returnedEventCount++;

                            storedEvent.EventType = storedEvent.Data.GetType().Name;

                            dsession.Store(storedEvent);
                            dsession.Advanced.GetMetadataFor(storedEvent)[Constants.RavenEntityName] = "Events";
                            dsession.SaveChanges();
                            Console.Write("*");
                        }

                        if (returnedEventCount%100 ==0)
                        {
                            Console.WriteLine();

                            RavenQueryStatistics stat;

                            var @event = dsession
                            .Advanced.LuceneQuery<StoredEvent>("Raven/DocumentsByEntityName")
                            .Statistics(out stat)
                            .Where("Tag:Events")
                            .Take(1)
                            .SingleOrDefault();

                            Console.WriteLine(stat.TotalResults);

                            Console.WriteLine();
                        }
                    }
                }
                
            } while (!allEventsWereReturned);
        }
    }
}
