using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneQuestionById")]
    public class CloneQuestionByIdCommand : QuestionCommand 
    {
        public Guid TargetId { get; private set; }

        public CloneQuestionByIdCommand(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            Guid targetId)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.TargetId = targetId;
        }
    }
}