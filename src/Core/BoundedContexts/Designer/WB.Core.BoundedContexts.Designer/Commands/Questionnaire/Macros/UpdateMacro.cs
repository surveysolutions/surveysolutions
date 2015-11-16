using System;

using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class UpdateMacro : QuestionnaireCommand
    {
        public UpdateMacro(Guid questionnaireId, Guid macroId, string name, string content, string description, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.MacroId = macroId;
            this.Name = name;
            this.Content = content;
            this.Description = description;
        }

        public Guid MacroId { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}