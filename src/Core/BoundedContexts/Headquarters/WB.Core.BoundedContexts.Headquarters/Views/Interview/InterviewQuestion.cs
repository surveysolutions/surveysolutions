using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion()
        {
            this.Valid = true;
            this.Enabled = true;
            this.IsAnswered = false;
            this.Comments = new List<InterviewQuestionComment>();
        }
        public InterviewQuestion(Guid id):this()
        {
            this.Id = id;
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
