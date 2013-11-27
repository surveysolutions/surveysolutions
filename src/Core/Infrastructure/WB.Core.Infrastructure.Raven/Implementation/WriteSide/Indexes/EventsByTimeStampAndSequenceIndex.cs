using System.Linq;
using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace WB.Core.Infrastructure.Raven.Implementation.WriteSide.Indexes
{
    internal class EventsByTimeStampAndSequenceIndex : AbstractIndexCreationTask<StoredEvent>
    {
        public EventsByTimeStampAndSequenceIndex()
        {
            Map = sEvents => from sEvent in sEvents
                             select
                                 new
                                     {
                                         sEvent.EventTimeStamp,
                                         sEvent.EventSequence,
                                         sEvent.EventIdentifier
                                     };
         
            Sort(x => x.EventSequence, SortOptions.Long);
            
            Index(x => x.EventIdentifier, FieldIndexing.NotAnalyzed);
          

        }
    }

}
