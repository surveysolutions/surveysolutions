using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide.Indexes
{
    internal class Event_ByEventSource : AbstractIndexCreationTask<StoredEvent>
    {
        public Event_ByEventSource()
        {
            this.Map = sEvents => from sEvent in sEvents
                             select
                                 new
                                     {
                                         sEvent.EventSourceId,
                                         sEvent.EventSequence,
                                         sEvent.EventIdentifier
                                     };
         
            this.Sort(x => x.EventSequence, SortOptions.Long);

            this.Index(x => x.EventSourceId, FieldIndexing.NotAnalyzed);
            this.Index(x => x.EventIdentifier, FieldIndexing.NotAnalyzed);
          //  Index(x => x.EventSequence, FieldIndexing.Analyzed);

        }
    }

}
