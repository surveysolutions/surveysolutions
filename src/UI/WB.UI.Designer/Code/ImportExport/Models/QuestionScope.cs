using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestionScope
    {
        Interviewer = 0, 
        Supervisor = 1, 
        Headquarter = 2,
        Hidden = 3, 
    }
}