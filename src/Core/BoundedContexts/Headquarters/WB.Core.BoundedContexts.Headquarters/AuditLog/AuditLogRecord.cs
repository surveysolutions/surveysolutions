using System;
using WB.Core.SharedKernels.DataCollection.Views.AuditLog;

namespace WB.Core.BoundedContexts.Headquarters.AuditLog
{
    public class AuditLogRecord
    {
        public int Id { get; set; }

        public int RecordId { get; set; }
        public Guid ResponsibleId { get; set; }

        public AuditLogEntityType Type { get; set; }

        public DateTime Time { get; set; }

        public DateTime TimeUtc { get; set; }

        public IAuditLogEntity Payload { get; set; }
    }
}
