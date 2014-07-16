using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireEntityCloneCommand : QuestionnaireEntityCommand
    {
        protected QuestionnaireEntityCloneCommand(Guid questionnaireId, Guid entityId, Guid responsibleId,
            Guid sourceEntityId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.SourceEntityId = sourceEntityId;
        }

        public Guid SourceEntityId { get; set; }
    }
}