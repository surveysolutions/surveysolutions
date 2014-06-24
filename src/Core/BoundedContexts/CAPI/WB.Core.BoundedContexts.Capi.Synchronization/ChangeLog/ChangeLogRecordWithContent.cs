using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog
{
    public class ChangeLogRecordWithContent : ChangeLogShortRecord
    {
        public ChangeLogRecordWithContent(Guid recordId, Guid eventSourceId, string content)
            : base(recordId, eventSourceId)
        {
            this.Content = content;
        }

        public string Content { get; private set; }
    }
}