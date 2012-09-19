// -----------------------------------------------------------------------
// <copyright file="UniqueEventsIndex.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Client.Indexes;

namespace Ncqrs.Restoring.EventStapshoot.EventStores.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class UniqueEventsIndex : AbstractIndexCreationTask<StoredEvent, UniqueEventsResults>
    {
        public UniqueEventsIndex()
        {
            Map = docs => from doc in docs
                          select new
                                     {
                                         EventSourceId = doc.EventSourceId,
                                         Count = 1
                                     };
            Reduce = results => from result in results
                                group result by result.EventSourceId
                                into g
                                select new
                                           {
                                               EventSourceId = g.Key,
                                               Count = g.Sum(x => x.Count)
                                           };
        }
    }
    public class UniqueEventsResults
    {
        public Guid EventSourceId { get; set; }
        public int Count { get; set; }
    }
}
