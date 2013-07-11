using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteQuestion")]
    public class DeleteQuestionCommand : QuestionCommand
    {
        public DeleteQuestionCommand(Guid questionnaireId, Guid questionId)
            : base(questionnaireId, questionId) {}
    }
}