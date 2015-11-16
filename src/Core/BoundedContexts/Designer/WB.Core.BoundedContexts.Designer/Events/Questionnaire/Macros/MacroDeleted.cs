using System;

using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros
{
    public class MacroDeleted : QuestionnaireActiveEvent
    {
        public MacroDeleted() { }
        public MacroDeleted(Guid macroId, Guid responsibleId)
        {
            this.MacroId = macroId;
            this.ResponsibleId = responsibleId;
        }

        public Guid MacroId { get; set; }
    }
}