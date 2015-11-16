using System;

using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class DeleteMacro : QuestionnaireCommand
    {
        public DeleteMacro(Guid questionnaireId, Guid macroId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.MacroId = macroId;
        }

        public Guid MacroId { get; private set; }
    }
}