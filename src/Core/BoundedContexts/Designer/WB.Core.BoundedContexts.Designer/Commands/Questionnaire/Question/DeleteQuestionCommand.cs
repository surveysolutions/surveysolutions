using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteQuestion")]
    public class DeleteQuestionCommand : QuestionCommand
    {
        public DeleteQuestionCommand(Guid questionnaireId, Guid questionId)
            : base(questionnaireId, questionId) {}
    }
}