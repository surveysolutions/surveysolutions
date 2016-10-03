using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.Macros
{
    public class MacroAdded : QuestionnaireActive
    {
        public MacroAdded(){ }

        public MacroAdded(Guid macroId, Guid responsibleId)
        {
            this.MacroId = macroId;
            this.ResponsibleId = responsibleId;
        }

        public Guid MacroId { get; set; }
    }
}
