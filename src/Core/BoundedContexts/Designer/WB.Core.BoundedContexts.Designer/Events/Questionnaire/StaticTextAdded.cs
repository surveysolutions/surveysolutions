using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class StaticTextAdded : QuestionnaireEntityEvent
    {
        public Guid ParentId { get; set; }
        public string Text { get; set; }
    }
}
