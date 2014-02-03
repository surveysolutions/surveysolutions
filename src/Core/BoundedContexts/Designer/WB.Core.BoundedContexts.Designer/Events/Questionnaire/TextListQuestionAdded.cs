using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class TextListQuestionAdded : AbstractListQuestionDataEvent
    {
        public Guid GroupId { get; set; }
    }
}
