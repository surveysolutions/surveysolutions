using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    public class UpdateStaticText : QuestionnaireEntityCommand
    {
        public UpdateStaticText(Guid questionnaireId, Guid entityId, string text, string attachmentName, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.Text = CommandUtils.SanitizeHtml(text);
            this.AttachmentName = attachmentName;
        }

        public string Text { get; set; }
        public string AttachmentName { get; set; }
    }
}
