using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class SingleOptionDetailsView : QuestionDetailsView
    {
        public SingleOptionDetailsView()
        {
            Type = QuestionType.SingleOption;
        }
        public Guid? LinkedToQuestionId { get; set; }
        public CategoricalOption[] Options { get; set; }
        public override QuestionType Type { get; set; }
        public bool? IsFilteredCombobox { get; set; }
    }
}