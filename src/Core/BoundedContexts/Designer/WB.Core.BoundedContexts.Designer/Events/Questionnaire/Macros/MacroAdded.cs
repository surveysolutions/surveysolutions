using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros
{
    public class MacroAdded : QuestionnaireActiveEvent
    {
        public MacroAdded(){ }

        public MacroAdded(Guid macroId, Guid responsibleId)
        {
            this.MacroId = macroId;
            ResponsibleId = responsibleId;
        }

        public Guid MacroId { get; set; }
    }
}
