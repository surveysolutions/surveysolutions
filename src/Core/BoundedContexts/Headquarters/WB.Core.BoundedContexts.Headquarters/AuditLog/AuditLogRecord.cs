using System;
using WB.Core.SharedKernels.DataCollection.Views.AuditLog;

namespace WB.Core.BoundedContexts.Headquarters.AuditLog
{
    public class AuditLogRecord
    {
        public virtual int Id { get; set; }

        public virtual int RecordId { get; set; }
        public virtual Guid? ResponsibleId { get; set; }
        public virtual string ResponsibleName { get; set; }

        public virtual AuditLogEntityType Type { get; set; }

        public virtual DateTime Time { get; set; }

        public virtual DateTime TimeUtc { get; set; }

        public virtual IAuditLogEntity Payload { get; set; }
    }
}
