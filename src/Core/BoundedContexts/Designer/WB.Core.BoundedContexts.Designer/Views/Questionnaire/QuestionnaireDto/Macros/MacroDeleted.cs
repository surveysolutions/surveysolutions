using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.Macros
{
    public class MacroDeleted : QuestionnaireActive
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