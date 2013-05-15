using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding.CommandExecution.Mapping.Fluent;
using Raven.Client.Indexes;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class UniqueEventsIndex : AbstractIndexCreationTask<StoredEvent, UniqueEventsResults>
    {
        public UniqueEventsIndex()
        {
            Map = docs => from doc in docs
                          // where doc.IsSnapshot
                          select new
                              {
                                  EventSourceId = doc.EventSourceId,
                                  LastSnapshot = doc.IsSnapshot ? doc.EventSequence : 1
                              };
            Reduce = results => from result in results
                                group result by result.EventSourceId
                                into g
                                select new
                                    {
                                        EventSourceId = g.Key,
                                        LastSnapshot = g.Max(e => e.LastSnapshot)
                                    };
        }
    }
    public class UniqueEventsResults
    {
        public Guid EventSourceId { get; set; }
        public int LastSnapshot { get; set; }
    }
}
