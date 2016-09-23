using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public abstract class Group : QuestionnaireActive
    {
        public Guid GroupId { get; private set; }

        protected Group(Guid responsibleId, Guid groupId)
            : base(responsibleId)
        {
            this.GroupId = groupId;
        }
    }
}