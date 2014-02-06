using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public abstract class AbstractQuestionAdded : AbstractQuestionUpdated
    {
        public Guid ParentGroupId { get; set; }
    }
}
