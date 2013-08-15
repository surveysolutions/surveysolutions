using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion(Guid id)
        {
            Id = id;
            Valid = true;
            Enabled = true;
            Flagged = false;
        }

        public InterviewQuestion(Guid id, object answer, string comments, bool valid, bool enabled, bool flagged)
        {
            Id = id;
            Answer = answer;
            Comments = comments;
            Valid = valid;
            Enabled = enabled;
            Flagged = flagged;
        }

        public Guid Id { get; private set; }
        public object Answer { get;  set; }
        public string Comments { get;  set; }
        public bool Valid { get;  set; }
        public bool Enabled { get;  set; }
        public bool Flagged { get;  set; }

        
    }
}
