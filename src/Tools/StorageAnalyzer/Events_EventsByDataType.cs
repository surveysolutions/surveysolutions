using System.Linq;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.Storage.Raven;

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
                                         Type = sEvent.EventType,
                                         Count = 1
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
