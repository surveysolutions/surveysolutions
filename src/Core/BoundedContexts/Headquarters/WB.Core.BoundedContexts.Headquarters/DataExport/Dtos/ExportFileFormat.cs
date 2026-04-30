using System.Text.Json.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportFileFormat
    {
        Zip = 1,
        TarGz = 2,
    }
}
