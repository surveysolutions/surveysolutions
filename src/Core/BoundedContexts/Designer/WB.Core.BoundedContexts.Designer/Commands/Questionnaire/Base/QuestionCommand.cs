using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionCommand : QuestionnaireCommand
    {
        protected QuestionCommand(Guid questionnaireId, Guid questionId, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.QuestionId = questionId;
        }

        public Guid QuestionId { get; private set; }
    }
}