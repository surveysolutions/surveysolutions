using System;
using System.Collections.Generic;
using WB.Services.Export.Interview.Entities;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Interview
{
    public class InterviewData 
    {
        public InterviewData()
        {
            this.Levels = new Dictionary<string, InterviewLevel>();
        }

        public virtual Guid InterviewId { get; set; }

        public virtual Guid ResponsibleId { get; set; }
        public virtual InterviewStatus Status { get; set; }
        public virtual int ErrorsCount { get; set; }

        public UserRoles ResponsibleRole { get; set; }
        public DateTime UpdateDate { get; set; }
        public Dictionary<string, InterviewLevel> Levels { get; set; }
        public bool WasCompleted { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool CreatedOnClient { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public string CurrentLanguage { get; set; } = String.Empty;
        public bool IsMissingAssignToInterviewer { get; set; }

        public string InterviewKey { get; set; } = String.Empty;
        public int? AssignmentId { get; set; }
    }
}
