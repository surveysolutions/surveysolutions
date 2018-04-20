using System;
using SQLite;

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

        public IAuditLogEntity Payload { get; set; }
    }
}
