using System;

using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros
{
    public class MacroDeleted : QuestionnaireEntityEvent
    {
        public MacroDeleted() { }
        public MacroDeleted(Guid entityId, Guid responsibleId)
        {
            this.EntityId = entityId;
            this.ResponsibleId = responsibleId;
        }
    }
}