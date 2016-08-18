namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    public enum DataExportStatus
    {
        NotStarted = 1,
        Queued,
        Running,
        Finished,
        FinishedWithError
    }
}