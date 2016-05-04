using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class TextListQuestionCloned : AbstractListQuestionDataEvent
    {
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceQuestionId { get; set; }
        public Guid GroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}
