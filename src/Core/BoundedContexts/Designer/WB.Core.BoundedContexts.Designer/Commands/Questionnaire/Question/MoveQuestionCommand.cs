using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "MoveQuestion")]
    public class MoveQuestionCommand : QuestionCommand
    {
        public MoveQuestionCommand(Guid questionnaireId, Guid questionId, Guid targetGroupId, int targetIndex)
            : base(questionnaireId, questionId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid TargetGroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}