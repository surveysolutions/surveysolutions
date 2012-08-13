using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Events;

namespace Web.Supervisor.Synchronization
{
    /// <summary>
    /// TODO please change logic here when you will strart supervisor synchronization implementation
    /// at this moment synchronizer will return all events from event store. it can harm perfomance.
    /// BE CAREFLL!!!!!
    /// </summary>
    public class SupervisorEventSync : AbstractEventSync
    {
        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEventStream> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            var allEvents = myEventStore.ReadFrom(DateTime.MinValue);
            List<Guid> aggregateRootIds = allEvents.GroupBy(x => x.EventSourceId).Select(x => x.Key).ToList();
            List<AggregateRootEventStream> retval = new List<AggregateRootEventStream>(aggregateRootIds.Count);
            foreach (Guid aggregateRootId in aggregateRootIds)
            {
                Guid id = aggregateRootId;
                retval.Add(new AggregateRootEventStream(new CommittedEventStream(aggregateRootId, allEvents.Where(e => e.EventSourceId == id).
                                                                              OrderBy(e => e.EventSequence))));
            }
            return retval;
        }

        #endregion
    }
}
