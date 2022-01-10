using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestionType
    {
        SingleOption = 0,

        MultiOptions = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11,

        Area = 12,

        Audio = 13
    }
}