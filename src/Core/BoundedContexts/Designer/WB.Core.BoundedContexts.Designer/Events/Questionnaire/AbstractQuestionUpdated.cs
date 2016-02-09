using System;
using Main.Core.Events.Questionnaire;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public abstract class AbstractQuestionUpdated : QuestionnaireActiveEvent
    {
        public Guid QuestionId { get; set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string Instructions { get; set; }
        public string Title { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }

        public QuestionScope QuestionScope { get; set; }
    }
}
