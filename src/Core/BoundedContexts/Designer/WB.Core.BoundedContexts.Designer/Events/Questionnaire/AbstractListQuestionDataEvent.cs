using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractListQuestionDataEvent : QuestionnaireActiveEvent
    {
        public string ConditionExpression { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public string StataExportCaption { get; set; }
        public string VariableLabel { get; set; }
        public int? MaxAnswerCount { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
    }
}
