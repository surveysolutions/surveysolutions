using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class MoveQuestion : QuestionCommand
    {
        public MoveQuestion(Guid questionnaireId, Guid questionId, Guid targetGroupId, int targetIndex, Guid responsibleId)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid TargetGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}