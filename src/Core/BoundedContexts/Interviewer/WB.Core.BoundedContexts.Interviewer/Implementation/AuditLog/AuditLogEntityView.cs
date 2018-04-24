using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Views.AuditLog;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public class AuditLogEntityView 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public AuditLogEntityType Type { get; set; }

        public DateTime Time { get; set; }
        public DateTime TimeUtc { get; set; }

        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }

        public IAuditLogEntity Payload { get; set; }
    }
}
