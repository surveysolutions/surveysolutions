using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
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
