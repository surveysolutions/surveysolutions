using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Order
    {
        AsIs, 
        Random, 
        AZ, 
        ZA, 
        MinMax, 
        MaxMin
    }
}