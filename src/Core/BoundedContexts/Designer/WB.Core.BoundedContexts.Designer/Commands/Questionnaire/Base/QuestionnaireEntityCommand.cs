using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireEntityCommand : QuestionnaireCommand
    {
        protected QuestionnaireEntityCommand(Guid questionnaireId, Guid responsibleId, Guid entityId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.EntityId = entityId;
        }

        public Guid EntityId { get; private set; }
    }
}