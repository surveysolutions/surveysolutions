using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserToVerify
    {
        public bool IsLocked { get; set; }
        public bool IsSupervisorOrInterviewer => InterviewerId.HasValue || SupervisorId.HasValue;
        public Guid? InterviewerId { get; set; } 
        public Guid? SupervisorId { get; set; }
    }
}
