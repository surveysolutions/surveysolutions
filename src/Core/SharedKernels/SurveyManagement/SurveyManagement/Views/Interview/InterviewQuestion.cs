using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion()
        {
            this.Invalid = false;
            this.Disabled = false;
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
        public bool Invalid { get;  set; }
        public bool Disabled { get;  set; }
        public bool IsFlagged { get; set; }
        public bool IsAnswered { get; set; }
    }
}
