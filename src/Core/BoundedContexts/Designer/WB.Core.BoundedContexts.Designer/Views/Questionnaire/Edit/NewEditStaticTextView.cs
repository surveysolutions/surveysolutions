using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditStaticTextView
    {

        public NewEditStaticTextView()
        {
            this.ValidationConditions = new List<ValidationCondition>();
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
