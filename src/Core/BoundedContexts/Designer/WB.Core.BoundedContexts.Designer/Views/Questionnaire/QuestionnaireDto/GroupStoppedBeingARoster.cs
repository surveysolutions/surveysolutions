using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class GroupStoppedBeingARoster : Group
    {
        public GroupStoppedBeingARoster(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId) {}
    }
}