using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace Ncqrs.Restoring.EventStapshoot.EventStores
{
    public interface ISnapshootEventStore:IEventStore
    {
        CommittedEvent GetLatestSnapshoot(Guid id);
    }
}
