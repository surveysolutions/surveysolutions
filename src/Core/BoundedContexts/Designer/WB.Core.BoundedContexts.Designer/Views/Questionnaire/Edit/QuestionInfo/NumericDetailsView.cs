using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class NumericDetailsView : QuestionDetailsView
    {
        public NumericDetailsView()
        {
            Type = QuestionType.Numeric;
        }

        public bool IsInteger { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public override QuestionType Type { get; set; }
    }
}