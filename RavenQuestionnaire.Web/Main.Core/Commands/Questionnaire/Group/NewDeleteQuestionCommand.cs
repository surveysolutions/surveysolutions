namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteQuestion")]
    public class NewDeleteQuestionCommand : QuestionCommand
    {
        public NewDeleteQuestionCommand(Guid questionnaireId, Guid questionId)
            : base(questionnaireId, questionId) {}
    }
}