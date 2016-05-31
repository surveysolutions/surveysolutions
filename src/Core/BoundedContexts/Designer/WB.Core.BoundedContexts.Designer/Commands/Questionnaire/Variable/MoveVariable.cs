using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable
{
    [Serializable]
    public class MoveVariable : QuestionnaireEntityMoveCommand
    {
        public MoveVariable(Guid questionnaireId, Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
            : base(questionnaireId, entityId, targetEntityId, targetIndex, responsibleId)
        {
        }
    }
}