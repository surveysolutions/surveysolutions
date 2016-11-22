using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewData : InterviewBrief
    {
        public InterviewData()
        {
            this.Levels = new Dictionary<string, InterviewLevel>();
        }

        public UserRoles ResponsibleRole { get; set; }
        public DateTime UpdateDate { get; set; }
        public Dictionary<string, InterviewLevel> Levels { get; set; }
        public bool WasCompleted { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool CreatedOnClient { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public string CurrentLanguage { get; set; }
        public bool IsAssignedToInterviewer { get; set; }
    }
}