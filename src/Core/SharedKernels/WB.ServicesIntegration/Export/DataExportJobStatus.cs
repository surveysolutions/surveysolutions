using System.Text.Json.Serialization;

namespace WB.ServicesIntegration.Export
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataExportJobStatus
    {
        Created = 0,
        Running = 1,
        Completed = 2,
        Fail = 3,
        Canceled = 4
    }
}
