using System;
using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class QuestionnaireVariableEvent : QuestionnaireEntityEvent
    {
        public QuestionnaireVariableEvent(Guid entityId, Guid responsibleId, VariableData variableData)
        {
            this.EntityId = entityId;
            this.ResponsibleId = responsibleId;

            this.VariableData = variableData;
        }

        public VariableData VariableData { get; }
    }
}