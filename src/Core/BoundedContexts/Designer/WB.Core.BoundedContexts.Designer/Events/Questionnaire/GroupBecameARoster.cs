using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class GroupBecameARoster : GroupEvent
    {
        public GroupBecameARoster(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId) {}
    }
}