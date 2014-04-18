using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class QrBarcodeDetailsView : QuestionDetailsView
    {
        public QrBarcodeDetailsView()
        {
            Type = QuestionType.QRBarcode;
        }
        public override sealed QuestionType Type { get; set; }
    }
}