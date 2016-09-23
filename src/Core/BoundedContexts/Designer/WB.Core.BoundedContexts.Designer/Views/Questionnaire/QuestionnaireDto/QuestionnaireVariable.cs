using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class QuestionnaireVariable : QuestionnaireEntity
    {
        public QuestionnaireVariable(Guid entityId, Guid responsibleId, VariableData variableData)
        {
            this.EntityId = entityId;
            this.ResponsibleId = responsibleId;

            this.VariableData = variableData;
        }

        public VariableData VariableData { get; }
    }
}