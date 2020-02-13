using System.Text.Json.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportFileType
    {
        Excel = 1,
        Csv,
        Tab
    }
}
