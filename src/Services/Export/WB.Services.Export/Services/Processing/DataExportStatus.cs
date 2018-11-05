namespace WB.Services.Export.Services.Processing
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
