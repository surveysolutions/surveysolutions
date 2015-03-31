using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public abstract class AbstractQuestionUpdated : QuestionnaireActiveEvent
    {
        public Guid QuestionId { get; set; }
        public string EnablementCondition { get; set; }
        public string Instructions { get; set; }
        public bool IsMandatory { get; set; }
        public string Title { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
    }
}
