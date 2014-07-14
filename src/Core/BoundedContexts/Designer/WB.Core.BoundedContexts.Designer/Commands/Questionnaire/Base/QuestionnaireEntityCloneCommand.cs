using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireEntityCloneCommand : QuestionnaireEntityCommand
    {
        protected QuestionnaireEntityCloneCommand(Guid questionnaireId, Guid entityId, Guid responsibleId,
            Guid parentId, Guid sourceEntityId, int targetIndex)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.SourceEntityId = sourceEntityId;
            this.TargetIndex = targetIndex;
        }

        public int TargetIndex { get; set; }
        public Guid SourceEntityId { get; set; }
    }
}