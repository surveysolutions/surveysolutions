namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewUpdateGroup")]
    public class NewUpdateGroupCommand : CommandBase
    {
        public NewUpdateGroupCommand(Guid questionnaireId, Guid groupId, string title)
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupId = groupId;
            this.Title = title;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public Guid GroupId { get; set; }

        public string Title { get; set; }
    }
}