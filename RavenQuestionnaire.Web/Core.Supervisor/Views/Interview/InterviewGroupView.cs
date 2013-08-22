using System;
using System.Collections.Generic;

namespace Core.Supervisor.Views.Interview
{
    public class InterviewGroupView 
    {
        public int Depth { set; get; }
        public Guid? ParentId { set; get; }
        public string Title { get; set; }

        public int[] PropagationVector { get; set; }

        public InterviewGroupView(Guid id)
        {
            Questions = new List<InterviewQuestionView>();
            Id = id;
        }
        public Guid Id { set; get; }
        public List<InterviewQuestionView> Questions { set; get; }
    }
}
