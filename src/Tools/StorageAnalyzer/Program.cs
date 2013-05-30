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

            var eventsInfo = new List<Events_EventsByDataType.ReduceResult>();

            using (IDocumentSession session = store.OpenSession())
            {
                eventsInfo.AddRange(session.Query<Events_EventsByDataType.ReduceResult, Events_EventsByDataType>()
                    .Customize(x=>x.WaitForNonStaleResultsAsOfNow())
                    .ToList()
                    .OrderBy(x=>x.Count));
            }

            foreach (var reduceResult in eventsInfo)
            {
                Console.WriteLine("{0,6}: {1}", reduceResult.Count, reduceResult.Type);
            }
        }
    }
}
