using System;

using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class UpdateMacroCommand : QuestionnaireEntityCommand
    {
        public UpdateMacroCommand(Guid questionnaireId, Guid entityId, string name, string content, string description, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.Name = name;
            this.Content = content;
            this.Description = description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}