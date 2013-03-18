namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    public class FullGroupDataCommand : CommandBase
    {
        public FullGroupDataCommand(Guid questionnaireId, Guid groupId, string title, Propagate propagationKind, string description, string condition)
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupId = groupId;
            this.Title = title;
            this.PropagationKind = propagationKind;
            this.Description = description;
            this.Condition = condition;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public Guid GroupId { get; set; }

        public string Title { get; set; }

        public Propagate PropagationKind { get; set; }

        public string Description { get; set; }

        public string Condition { get; set; }
    }
}