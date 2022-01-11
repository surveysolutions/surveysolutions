using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    [NonWorkspaced]
    public class AuditLogEntityView 
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public AuditLogEntityType Type { get; set; }

        public DateTime Time { get; set; }
        public DateTime TimeUtc { get; set; }

        public Guid? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }

        public IAuditLogEntity Payload { get; set; }
        
        public string Workspace { get; set; }
    }
}
