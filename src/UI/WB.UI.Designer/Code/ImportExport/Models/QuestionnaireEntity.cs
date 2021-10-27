using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WB.UI.Designer.Code.ImportExport.Models.Question;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [JsonConverter(typeof(QuestionnaireEntityJsonConverter), "type")]
    [KnownType(typeof(TextQuestion))]
    [KnownType(typeof(NumericQuestion))]
    [KnownType(typeof(AreaQuestion))]
    [KnownType(typeof(AudioQuestion))]
    [KnownType(typeof(DateTimeQuestion))]
    [KnownType(typeof(GpsCoordinateQuestion))]
    [KnownType(typeof(MultimediaQuestion))]
    [KnownType(typeof(MultiOptionsQuestion))]
    [KnownType(typeof(QRBarcodeQuestion))]
    [KnownType(typeof(SingleQuestion))]
    [KnownType(typeof(TextListQuestion))]
    [KnownType(typeof(Group))]
    [KnownType(typeof(StaticText))]
    [KnownType(typeof(Variable))]
    public abstract class QuestionnaireEntity : IQuestionnaireEntity
    {
        public Guid Id { get; set; }
    }
}