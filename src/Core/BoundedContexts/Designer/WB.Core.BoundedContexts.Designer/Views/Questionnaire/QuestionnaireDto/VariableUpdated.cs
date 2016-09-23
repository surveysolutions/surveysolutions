using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class VariableUpdated : QuestionnaireVariable
    {
        public VariableUpdated(Guid entityId, Guid responsibleId, VariableData variableData)
            : base(entityId, responsibleId, variableData)
        {
        }
    }
}