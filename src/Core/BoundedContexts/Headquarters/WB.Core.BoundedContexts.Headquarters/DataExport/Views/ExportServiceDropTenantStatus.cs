namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views;

public class ExportServiceDropTenantStatus
{
    public DropTenantStatus Status { get; set; }
}

public enum DropTenantStatus
{
    Unknown = 0,
    NotStarted = 1,
    Removing,
    Removed,
    Error
}
