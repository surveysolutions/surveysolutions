using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewQuestionComment
    {
        public Guid Id { get; set; }

        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Guid CommenterId { get; set; }
        public string CommenterName { get; set; }
    }
}
