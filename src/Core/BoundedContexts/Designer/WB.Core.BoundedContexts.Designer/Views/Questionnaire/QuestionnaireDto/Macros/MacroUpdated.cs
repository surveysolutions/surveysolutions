using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.Macros
{
    public class MacroUpdated : QuestionnaireActive
    {
        public MacroUpdated() { }

        public MacroUpdated(Guid macroId, string name, string content, string description, Guid responsibleId)
        {
            this.MacroId = macroId;
            this.ResponsibleId = responsibleId;
            this.Name = name;
            this.Content = content;
            this.Description = description;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public Guid MacroId { get; set; }
    }
}