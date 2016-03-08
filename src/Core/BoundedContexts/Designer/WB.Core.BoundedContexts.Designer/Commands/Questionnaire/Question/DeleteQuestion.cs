using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class DeleteQuestion : QuestionCommand
    {
        public DeleteQuestion(Guid questionnaireId, Guid questionId, Guid responsibleId)
            : base(questionnaireId, questionId, responsibleId) {}
    }
}