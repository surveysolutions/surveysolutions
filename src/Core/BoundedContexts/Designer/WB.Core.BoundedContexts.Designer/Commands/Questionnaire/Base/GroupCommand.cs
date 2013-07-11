using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class GroupCommand : QuestionnaireCommand
    {
        protected GroupCommand(Guid questionnaireId, Guid groupId)
            : base(questionnaireId)
        {
            this.GroupId = groupId;
        }

        public Guid GroupId { get; set; }
    }
}