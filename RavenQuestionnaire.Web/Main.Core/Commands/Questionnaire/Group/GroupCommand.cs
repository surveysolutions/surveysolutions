namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

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