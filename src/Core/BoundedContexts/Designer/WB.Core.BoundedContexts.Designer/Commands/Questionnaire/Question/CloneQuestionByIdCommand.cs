using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
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