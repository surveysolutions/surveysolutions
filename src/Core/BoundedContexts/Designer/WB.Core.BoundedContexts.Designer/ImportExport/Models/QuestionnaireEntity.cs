using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.ImportExport.Models.Question;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [JsonConverter(typeof(QuestionnaireEntityJsonConverter), "Type")]
    [KnownType(typeof(TextQuestion))]
    [KnownType(typeof(NumericQuestion))]
    [KnownType(typeof(AreaQuestion))]
    [KnownType(typeof(AudioQuestion))]
    [KnownType(typeof(DateTimeQuestion))]
    [KnownType(typeof(GpsCoordinateQuestion))]
    [KnownType(typeof(PictureQuestion))]
    [KnownType(typeof(MultiOptionsQuestion))]
    [KnownType(typeof(QRBarcodeQuestion))]
    [KnownType(typeof(SingleQuestion))]
    [KnownType(typeof(TextListQuestion))]
    [KnownType(typeof(Group))]
    [KnownType(typeof(Roster))]
    [KnownType(typeof(StaticText))]
    [KnownType(typeof(Variable))]
    public abstract class QuestionnaireEntity : IQuestionnaireEntity
    {
        public Guid? Id { get; set; }
    }
}