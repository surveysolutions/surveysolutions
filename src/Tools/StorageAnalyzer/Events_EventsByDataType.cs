using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Ncqrs.Eventing.Storage.RavenDB.RavenIndexes
{
    public class Events_EventsByDataType : AbstractIndexCreationTask<StoredEvent, Events_EventsByDataType.ReduceResult>
    {
        public class ReduceResult
        {
            public string Type { get; set; }
            public int Count { get; set; }
        }

        public Events_EventsByDataType()
        {
            Map =  sEvents => from sEvent in sEvents
                             select
                                 new
                                     {
                                         Type = sEvent.EventSourceId.ToString(),
                                         Count = MetadataFor(sEvent).Value<int>("Content-Length")
                                         //Count = 1
                                     };

            Reduce = results => from result in results
                                group result by result.Type
                                    into g
                                    select new ReduceResult
                                        {
                                            Type = g.Key,
                                            Count = g.Sum(x => x.Count)
                                        };
        }
    }

}
