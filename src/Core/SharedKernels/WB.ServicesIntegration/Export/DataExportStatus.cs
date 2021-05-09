
namespace WB.ServicesIntegration.Export
{
    public enum DataExportStatus
    {
        Unknown = 0,
        NotStarted = 1,
        Queued = 2,
        Running = 3,
        Compressing = 4,
        Finished = 5,
        FinishedWithError = 6,
        Preparing = 7
    }
}
