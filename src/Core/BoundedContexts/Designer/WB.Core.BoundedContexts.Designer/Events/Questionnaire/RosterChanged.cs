using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class RosterChanged : GroupEvent
    {
        public Guid RosterSizeQuestionId { get; private set; }

        public RosterChanged(Guid responsibleId, Guid groupId, Guid rosterSizeQuestionId)
            : base(responsibleId, groupId)
        {
            this.RosterSizeQuestionId = rosterSizeQuestionId;
        }
    }
}