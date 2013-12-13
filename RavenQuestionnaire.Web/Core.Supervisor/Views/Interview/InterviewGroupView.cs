using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Core.Supervisor.Views.Interview
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
            Questions = new List<InterviewQuestionView>();
            Id = id;
        }
        public Guid Id { set; get; }
        public List<InterviewQuestionView> Questions { set; get; }
    }
}
