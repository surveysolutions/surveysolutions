using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class SingleOptionDetailsView : QuestionDetailsView
    {
        public readonly QuestionType QuestionType = QuestionType.SingleOption;
        public Guid? LinkedToQuestionId { get; set; }
        public List<CategoricalOption> Options { get; set; }
    }
}