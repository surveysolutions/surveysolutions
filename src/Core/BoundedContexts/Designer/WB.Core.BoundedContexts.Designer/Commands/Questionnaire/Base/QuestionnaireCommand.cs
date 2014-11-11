using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireCommand : QuestionnaireCommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId)
            : base(responsibleId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}