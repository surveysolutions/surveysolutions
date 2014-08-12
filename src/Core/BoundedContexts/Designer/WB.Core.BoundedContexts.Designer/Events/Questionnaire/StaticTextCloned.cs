using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class StaticTextCloned : QuestionnaireEntityEvent
    {
        public Guid ParentId { get; set; }
        public Guid SourceEntityId { get; set; }
        public int TargetIndex { get; set; }
        public string Text { get; set; }
    }
}
