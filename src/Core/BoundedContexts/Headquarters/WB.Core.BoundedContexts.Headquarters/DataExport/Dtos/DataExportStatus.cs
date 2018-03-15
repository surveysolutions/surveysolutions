namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    public enum DataExportStatus
    {
        NotStarted = 1,
        Queued = 2,
        Running = 3,
        Compressing = 4,
        Finished = 5,
        FinishedWithError = 6
    }
}