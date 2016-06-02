using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class VariableAdded : QuestionnaireVariableEvent
    {
        public VariableAdded(Guid entityId, Guid responsibleId, Guid parentId, VariableData variableData)
            : base(entityId, responsibleId, variableData)
        {
            this.ParentId = parentId;
        }

        public Guid ParentId { get; set; }
    }
}