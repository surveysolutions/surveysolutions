using System.Runtime.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Converters;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class TextQuestion : AbstractQuestion
    {
        public string? Mask { get; set; }
    }
}
