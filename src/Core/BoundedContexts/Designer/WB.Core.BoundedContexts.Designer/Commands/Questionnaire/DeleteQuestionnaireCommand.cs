namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteQuestionnaire")]
    public class DeleteQuestionnaireCommand : CommandBase
    {
        public DeleteQuestionnaireCommand(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
    }
}