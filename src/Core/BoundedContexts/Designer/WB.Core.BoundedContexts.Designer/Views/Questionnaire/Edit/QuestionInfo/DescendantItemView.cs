using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public abstract class DescendantItemView
    {
        protected DescendantItemView()
        {
            this.ParentGroupsIds = new Guid[0];
            this.RosterScopeIds = new Guid[0];
        }

        public Guid Id { get; set; }
        public Guid[] ParentGroupsIds { get; set; }
        public Guid[] RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
    }
}