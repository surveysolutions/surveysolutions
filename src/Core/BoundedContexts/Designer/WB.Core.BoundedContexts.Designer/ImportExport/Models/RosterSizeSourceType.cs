using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RosterSizeSourceType
    {
        Question = 0,
        FixedTitles = 1
    }
}
