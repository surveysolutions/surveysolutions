using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class GroupBecameARoster : Group
    {
        public GroupBecameARoster(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId) {}
    }
}