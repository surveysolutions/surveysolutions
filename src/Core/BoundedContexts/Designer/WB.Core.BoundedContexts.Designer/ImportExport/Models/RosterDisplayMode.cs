using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RosterDisplayMode
    {
        SubSection = 0,
        Flat = 1,
        Table = 2,
        Matrix = 3
    }
}
