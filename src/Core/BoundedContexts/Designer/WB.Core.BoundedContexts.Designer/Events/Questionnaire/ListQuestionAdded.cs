using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class ListQuestionAdded : AbstractListQuestionDataEvent
    {
        public Guid GroupPublicKey { get; set; }
    }
}
