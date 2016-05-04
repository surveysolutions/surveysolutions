using System;
using System.Collections.Generic;
using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class StaticTextAdded : QuestionnaireEntityEvent
    {
        public StaticTextAdded(Guid entityId, Guid responsibleId, Guid parentId, string text, string enablementCondition, bool hideIfDisabled, IList<ValidationCondition> validationConditions) 
        {
            this.EntityId = entityId;
            this.ResponsibleId = responsibleId;
            this.ParentId = parentId;
            this.Text = text;

            this.HideIfDisabled = hideIfDisabled;
            this.EnablementCondition = enablementCondition;
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>(); ;
        }

        public Guid ParentId { get; set; }
        public string Text { get; set; }

        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
