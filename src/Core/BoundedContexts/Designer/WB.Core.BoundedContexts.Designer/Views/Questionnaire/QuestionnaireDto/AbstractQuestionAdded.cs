using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public abstract class AbstractQuestionAdded : AbstractQuestionUpdated
    {
        public Guid ParentGroupId { get; set; }
    }
}
