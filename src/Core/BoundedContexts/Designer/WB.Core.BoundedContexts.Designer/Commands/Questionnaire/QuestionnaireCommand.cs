namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    public abstract class QuestionnaireCommand : CommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
    }
}