using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireEntityMoveCommand : QuestionnaireEntityCommand
    {
        protected QuestionnaireEntityMoveCommand(Guid questionnaireId, Guid entityId, Guid targetEntityId,
            int targetIndex, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.TargetEntityId = targetEntityId;
            this.TargetIndex = targetIndex;
        }

        public Guid TargetEntityId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}