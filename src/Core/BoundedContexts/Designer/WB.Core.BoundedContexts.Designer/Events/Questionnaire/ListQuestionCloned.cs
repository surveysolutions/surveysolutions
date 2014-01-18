using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class ListQuestionCloned : AbstractListQuestionDataEvent
    {
        public Guid SourceQuestionId { get; set; }
        public Guid GroupPublicKey { get; set; }
        public int TargetIndex { get; set; }
    }
}
