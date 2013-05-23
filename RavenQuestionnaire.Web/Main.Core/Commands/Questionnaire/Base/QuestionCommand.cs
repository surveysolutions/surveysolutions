using System;

namespace Main.Core.Commands.Questionnaire.Base
{
    public abstract class QuestionCommand : QuestionnaireCommand
    {
        protected QuestionCommand(Guid questionnaireId, Guid questionId)
            : base(questionnaireId)
        {
            this.QuestionId = questionId;
        }

        public Guid QuestionId { get; set; }
    }
}