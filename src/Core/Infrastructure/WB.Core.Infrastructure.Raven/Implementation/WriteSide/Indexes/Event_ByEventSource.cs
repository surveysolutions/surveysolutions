using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Ncqrs.Eventing.Storage.RavenDB.RavenIndexes
{
    internal class Event_ByEventSource : AbstractIndexCreationTask<StoredEvent>
    {
        public Event_ByEventSource()
        {
            Map = sEvents => from sEvent in sEvents
                             select
                                 new
                                     {
                                         sEvent.EventSourceId,
                                         sEvent.EventSequence,
                                         sEvent.EventIdentifier
                                     };
         
            Sort(x => x.EventSequence, SortOptions.Long);

            Index(x => x.EventSourceId, FieldIndexing.NotAnalyzed);
            Index(x => x.EventIdentifier, FieldIndexing.NotAnalyzed);
          //  Index(x => x.EventSequence, FieldIndexing.Analyzed);

        }
    }

}
