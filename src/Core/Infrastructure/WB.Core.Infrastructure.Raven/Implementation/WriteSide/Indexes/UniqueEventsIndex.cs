using System;
using System.Diagnostics;
using System.Linq;
using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Client.Indexes;

namespace WB.Core.Infrastructure.Raven.Implementation.WriteSide.Indexes
{
    internal class UniqueEventsIndex : AbstractIndexCreationTask<StoredEvent, UniqueEventsResults>
    {
        public UniqueEventsIndex()
        {
            Map = docs => from doc in docs
                          // where doc.IsSnapshot
                          select new
                              {
                                  EventSourceId = doc.EventSourceId,
                                  LastSnapshot = 1,
                                  EventTimeStamp = doc.EventTimeStamp
                              };
            Reduce = results => from result in results
                                group result by result.EventSourceId
                                into g
                                select new
                                    {
                                        EventSourceId = g.Key,
                                        LastSnapshot = g.Max(e => e.LastSnapshot),
                                        EventTimeStamp = g.Min(e => e.EventTimeStamp)
                                    };
        }
    }

    [DebuggerDisplay("UniqueEventsResults {EventSourceId}")]
    public class UniqueEventsResults
    {
        public Guid EventSourceId { get; set; }
        public int LastSnapshot { get; set; }
        public DateTime EventTimeStamp { get; set; }
    }
    
}
