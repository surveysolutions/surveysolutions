using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable
{
    [Serializable]
    public class UpdateVariable : QuestionnaireEntityCommand
    {
        public UpdateVariable(Guid questionnaireId, Guid responsibleId, Guid entityId, VariableData variableData)
            : base(questionnaireId, responsibleId, entityId)
        {
            this.VariableData = variableData;
            this.VariableData.Label = CommandUtils.SanitizeHtml(variableData.Label);
        }

        public VariableData VariableData { get; private set; }

    }
}