using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros
{
    public class MacroAdded : QuestionnaireEntityEvent
    {
        public MacroAdded(){ }

        public MacroAdded(Guid entityId, Guid responsibleId)
        {
            EntityId = entityId;
            ResponsibleId = responsibleId;
        }
    }
}
