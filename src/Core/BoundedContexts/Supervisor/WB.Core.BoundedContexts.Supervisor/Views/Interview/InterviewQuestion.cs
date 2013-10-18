using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewQuestion
    {
        

        public InterviewQuestion()
        {
            Valid = true;
            Enabled = true;
            IsAnswered = false;
            Comments = new List<InterviewQuestionComment>();
        }
        public InterviewQuestion(Guid id):this()
        {
            Id = id;
        }

        public Guid Id { get; private set; }
        public object Answer { get;  set; }
        public List<InterviewQuestionComment> Comments { get; set; }
        public bool Valid { get;  set; }
        public bool Enabled { get;  set; }
        public bool IsFlagged { get; set; }
        public bool IsAnswered { get; set; }
    }
}
