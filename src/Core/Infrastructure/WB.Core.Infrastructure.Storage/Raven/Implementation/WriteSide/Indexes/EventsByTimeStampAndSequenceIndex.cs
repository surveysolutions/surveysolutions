using System.Linq;
using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace WB.Core.Infrastructure.Raven.Raven.Implementation.WriteSide.Indexes
{
    internal class EventsByTimeStampAndSequenceIndex : AbstractIndexCreationTask<StoredEvent>
    {
        public EventsByTimeStampAndSequenceIndex()
        {
            this.Map = sEvents => from sEvent in sEvents
                             select
                                 new
                                     {
                                         sEvent.EventSourceId,
                                         sEvent.EventTimeStamp,
                                         sEvent.EventSequence,
                                         sEvent.EventIdentifier
                                     };
         
            this.Sort(x => x.EventSequence, SortOptions.Long);
            
            this.Index(x => x.EventIdentifier, FieldIndexing.NotAnalyzed);
          

        }
    }

}
