using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    public class UpdateStaticText : QuestionnaireEntityCommand
    {
        public UpdateStaticText(Guid questionnaireId, Guid entityId, string text, string attachmentName, Guid responsibleId, 
            string enablementCondition, bool hideIfDisabled, List<ValidationCondition>? validationConditions)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.Text = CommandUtils.SanitizeHtml(text);
            this.AttachmentName = attachmentName;
            this.EnablementCondition = enablementCondition;
            this.HideIfDisabled = hideIfDisabled;
            
            this.ValidationConditions = validationConditions ?? new (); 
            this.ValidationConditions.ForEach(x => CommandUtils.SanitizeHtml(x.Message, removeAllTags: false));
        }

        public string Text { get; set; }
        public string AttachmentName { get; set; }

        public bool HideIfDisabled { get; set; }
        public string EnablementCondition { get; set; }
        public List<ValidationCondition> ValidationConditions { get; set; }
    }
}
