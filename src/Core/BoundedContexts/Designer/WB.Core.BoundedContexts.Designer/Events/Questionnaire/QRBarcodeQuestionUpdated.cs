using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class QRBarcodeQuestionUpdated : AbstractQuestionUpdated
    {
        public QuestionScope QuestionScope { get; set; }
    }
}