using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    public class UpdateStaticTextCommand : QuestionnaireEntityCommand
    {
        public UpdateStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.Text = CommandUtils.SanitizeHtml(text);
        }

        public string Text { get; set; }
    }
}
