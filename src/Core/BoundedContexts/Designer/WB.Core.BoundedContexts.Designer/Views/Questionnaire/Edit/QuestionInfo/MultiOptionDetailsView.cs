using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class MultiOptionDetailsView : QuestionDetailsView
    {
        public MultiOptionDetailsView()
        {
            Type = QuestionType.MultyOption;
        }
        public override sealed QuestionType Type { get; set; }
        public Guid? LinkedToQuestionId { get; set; }
        public CategoricalOption[] Options { get; set; }
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
    }
}