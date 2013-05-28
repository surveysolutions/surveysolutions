using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Client;
using Raven.Client.Document;

namespace StorageAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var store = (new DocumentStore { Url = "http://localhost:8080" }).Initialize();
            int pageSize = 1024;
            int timeout = 120;
            IQueryable<StoredEvent> result = Enumerable.Empty<StoredEvent>().AsQueryable();
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    List<StoredEvent> chunk = session
                        .Query<StoredEvent>()
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                        .OrderBy(e => e.EventSequence)
                        .Skip(page * pageSize)
                        .Take(pageSize)
                        .ToList();

                    if (chunk.Count == 0)
                    {
                        break;
                    }

                    result = result.Concat(chunk);
                    page++;
                }
            }
            */
        }
    }
}
