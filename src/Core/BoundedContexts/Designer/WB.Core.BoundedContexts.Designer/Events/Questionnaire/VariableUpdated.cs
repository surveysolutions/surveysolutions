using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class VariableUpdated : QuestionnaireVariableEvent
    {
        public VariableUpdated(Guid entityId, Guid responsibleId, VariableData variableData)
            : base(entityId, responsibleId, variableData)
        {
        }
    }
}