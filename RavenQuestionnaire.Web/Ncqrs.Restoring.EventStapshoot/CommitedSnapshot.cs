using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class CommitedSnapshot : Ncqrs.Eventing.Sourcing.Snapshotting.Snapshot
    {
        public CommitedSnapshot(Guid eventSourceId, long version, object payload)
            : base(eventSourceId, version, payload)
        {
        }
    }
}
