using System;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class ReceivedPackageLogEntry
    {
        public virtual int Id { get; set; }

        public virtual Guid FirstEventId { get; set; }
        public virtual Guid LastEventId { get; set; }
        public virtual DateTime FirstEventTimestamp { get; set; }
        public virtual DateTime LastEventTimestamp { get; set; }
    }
}
