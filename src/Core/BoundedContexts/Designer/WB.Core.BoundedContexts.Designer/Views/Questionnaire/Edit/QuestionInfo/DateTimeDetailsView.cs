using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class DateTimeDetailsView : QuestionDetailsView
    {
        public DateTimeDetailsView()
        {
            Type = QuestionType.DateTime;
        }

        public bool IsTimestamp { get; set; }
        public override sealed QuestionType Type { get; set; }
    }
}