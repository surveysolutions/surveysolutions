using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditStaticTextView
    {

        public NewEditStaticTextView(Guid id, string? text = null, string? attachmentName = null, string? enablementCondition = null, 
            Breadcrumb[]? breadcrumbs = null, List<ValidationCondition>? validationCondition = null, bool hideIfDisabled = false)
        {
            ParentGroupsIds = new Guid[0];
            RosterScopeIds = new Guid[0];

            ValidationConditions = validationCondition ?? new List<ValidationCondition>();
            Id = id;
            HideIfDisabled = hideIfDisabled;
            Text = text ?? "";
            AttachmentName = attachmentName ?? "";
            EnablementCondition = enablementCondition ?? "";
            Breadcrumbs = breadcrumbs ?? new Breadcrumb[0];
        }

        public Guid Id { get; set; }
        public Guid[] ParentGroupsIds { get; set; }
        public Guid[] RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
        public string Text { get; set; }
        public string AttachmentName { get; set; }

        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public List<ValidationCondition> ValidationConditions { get; private set; }

        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}
