using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class GroupStoppedBeingARoster : GroupEvent
    {
        public GroupStoppedBeingARoster(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId) {}
    }
}