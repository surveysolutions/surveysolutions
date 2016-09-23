using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class StaticTextCloned : QuestionnaireEntity
    {
        public StaticTextCloned(Guid entityId, Guid responsibleId, Guid parentId, Guid? sourceQuestionnaireId, Guid sourceEntityId, int targetIndex, string text, 
            string attachmentName, string enablementCondition, bool hideIfDisabled, IList<ValidationCondition> validationConditions)
        {
            this.EntityId = entityId;
            this.ResponsibleId = responsibleId;

            this.ParentId = parentId;
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceEntityId = sourceEntityId;
            this.TargetIndex = targetIndex;
            this.Text = text;
            this.AttachmentName = attachmentName;
            this.EnablementCondition = enablementCondition;
            this.HideIfDisabled = hideIfDisabled;
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
        }

        public Guid ParentId { get; set; }
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceEntityId { get; set; }
        public int TargetIndex { get; set; }

        public string Text { get; set; }
        public string AttachmentName { get; internal set; }

        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
