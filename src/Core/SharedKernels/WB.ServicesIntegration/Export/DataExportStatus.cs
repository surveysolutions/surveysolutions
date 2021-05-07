using System.Text.Json.Serialization;

namespace WB.ServicesIntegration.Export
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataExportStatus
    {
        NotStarted = 1,
        Queued = 2,
        Running = 3,
        Compressing = 4,
        Finished = 5,
        FinishedWithError = 6,
        Preparing = 7
    }
}
