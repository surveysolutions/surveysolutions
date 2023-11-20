namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views;

public class ExportServiceDropTenantStatus
{
    public StopTenantStatus Status { get; set; }
}

public enum StopTenantStatus
{
    NotStarted = 1,
    Removing,
    Removed,
    Error
}
