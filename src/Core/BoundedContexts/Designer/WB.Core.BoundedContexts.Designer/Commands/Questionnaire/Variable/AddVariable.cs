using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable
{
    [Serializable]
    public class AddVariable : QuestionnaireEntityAddCommand
    {
        public AddVariable(Guid questionnaireId, Guid entityId, VariableData variableData, Guid responsibleId, Guid parentId, int? index = null)
            : base(questionnaireId, entityId, responsibleId, parentId)
        {
            Index = index;
            VariableData = variableData;
            variableData.Type = VariableData.Type == 0 ? VariableType.Boolean : variableData.Type;
        }

        public VariableData VariableData { get; private set; }
        public int? Index { get; private set; }
    }
}