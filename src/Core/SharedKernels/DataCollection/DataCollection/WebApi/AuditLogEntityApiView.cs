using System;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.SharedKernels.DataCollection.WebApi;

public class AuditLogEntityApiView
{
    public int Id { get; set; }

    public AuditLogEntityType Type { get; set; }

    public DateTimeOffset Time { get; set; }
    public DateTimeOffset TimeUtc { get; set; }

    public Guid? ResponsibleId { get; set; }
    public string ResponsibleName { get; set; }

    public string PayloadType { get; set; }
    public string Payload { get; set; }
    public string Workspace { get; set; }
}
