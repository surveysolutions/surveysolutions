using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    public class MoveStaticText : QuestionnaireEntityMoveCommand
    {
        public MoveStaticText(Guid questionnaireId, Guid entityId, Guid targetEntityId, int targetIndex,
            Guid responsibleId)
            : base(
                questionnaireId: questionnaireId, targetEntityId: targetEntityId, entityId: entityId,
                responsibleId: responsibleId, targetIndex: targetIndex)
        {
        }
    }
}