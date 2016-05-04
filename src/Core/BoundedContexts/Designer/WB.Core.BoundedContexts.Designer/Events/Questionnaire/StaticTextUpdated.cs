﻿using System;
using System.Collections.Generic;
using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class StaticTextUpdated : QuestionnaireEntityEvent
    {
        public StaticTextUpdated(Guid entityId, Guid responsibleId,  string text, string attachmentName, bool hideIfDisabled, 
            string enablementCondition, IList<ValidationCondition> validationConditions)
        {
            this.EntityId = entityId;

            this.AttachmentName = attachmentName;
            this.EnablementCondition = enablementCondition;
            this.Text = text;
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
            this.ResponsibleId = responsibleId;
            this.HideIfDisabled = hideIfDisabled;
        }

        public string Text { get; set; }
        public string AttachmentName { get; set; }

        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
