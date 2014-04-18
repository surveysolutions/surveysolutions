using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class MultiOptionDetailsView : QuestionDetailsView
    {
        public readonly QuestionType QuestionType = QuestionType.MultyOption;
        public Guid? LinkedToQuestionId { get; set; }
        public List<CategoricalOption> Options { get; set; }
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
    }
}