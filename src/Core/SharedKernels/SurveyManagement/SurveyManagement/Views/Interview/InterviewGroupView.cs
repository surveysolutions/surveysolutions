using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class InterviewGroupView 
    {
        public int Depth { set; get; }
        public Guid? ParentId { set; get; }
        public string Title { get; set; }

        public decimal[] RosterVector { get; set; }

        public InterviewGroupView(Guid id)
        {
            this.Entities = new List<InterviewEntityView>();
            this.Id = id;
        }
        public Guid Id { set; get; }
        public List<InterviewEntityView> Entities { set; get; }
    }
}
