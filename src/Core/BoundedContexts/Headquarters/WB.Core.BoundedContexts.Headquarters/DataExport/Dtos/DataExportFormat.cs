using System.Text.Json.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataExportFormat
    {
        Tabular = 1,
        STATA,
        SPSS,
        Binary,
        DDI,
        Paradata
    }
}
