using System;

namespace WB.Core.BoundedContexts.Interviewer.ChangeLog
{
    public class ChangeLogShortRecord
    {
        public ChangeLogShortRecord(Guid recordId, Guid eventSourceId)
        {
            this.RecordId = recordId;
            this.EventSourceId = eventSourceId;
        }

        public Guid RecordId { get; private set; }
        public Guid EventSourceId { get; private set; }
    }
}