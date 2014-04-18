using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class NumericDetailsView : QuestionDetailsView
    {
        public readonly QuestionType QuestionType = QuestionType.Numeric;
        public bool IsInteger { get; set; }
        public int? MaxValue { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
    }
}