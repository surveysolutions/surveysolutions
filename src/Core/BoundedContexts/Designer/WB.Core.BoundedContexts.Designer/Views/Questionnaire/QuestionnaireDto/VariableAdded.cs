using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class VariableAdded : QuestionnaireVariable
    {
        public VariableAdded(Guid entityId, Guid responsibleId, Guid parentId, VariableData variableData)
            : base(entityId, responsibleId, variableData)
        {
            this.ParentId = parentId;
        }

        public Guid ParentId { get; set; }
    }
}