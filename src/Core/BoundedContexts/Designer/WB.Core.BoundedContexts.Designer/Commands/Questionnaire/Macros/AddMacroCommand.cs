using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class AddMacroCommand : QuestionnaireEntityCommand
    {
        public AddMacroCommand(Guid questionnaireId, Guid entityId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
        }
    }
}
