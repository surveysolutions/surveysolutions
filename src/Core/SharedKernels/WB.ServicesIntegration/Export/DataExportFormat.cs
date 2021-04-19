using System.Text.Json.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataExportFormat
    {
        Tabular = 1,
        STATA = 2,
        SPSS = 3,
        Binary = 4,
        DDI = 5,
        Paradata = 6
    }
}
