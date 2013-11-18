using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public abstract class GroupEvent : QuestionnaireActiveEvent
    {
        public Guid GroupId { get; private set; }

        protected GroupEvent(Guid responsibleId, Guid groupId)
            : base(responsibleId)
        {
            this.GroupId = groupId;
        }
    }
}