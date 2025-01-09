namespace WB.Core.SharedKernels.DataCollection.WebApi;

public class AuditLogEntitiesApiView
{
    public bool IsWorkspaceSupported { get; set; } 
    public AuditLogEntityApiView[] Entities { get; set; }
}
