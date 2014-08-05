using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditStaticTextView
    {
        public Guid Id { get; set; }
        public Guid[] ParentGroupsIds { get; set; }
        public Guid[] RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
        public string Text { get; set; }
        
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}