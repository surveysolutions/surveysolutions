using System;

using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class DeleteMacro : QuestionnaireEntityCommand
    {
        public DeleteMacro(Guid questionnaireId, Guid entityId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
        }
    }
}