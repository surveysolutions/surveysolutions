using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserToVerify
    {
        public bool IsLocked { get; set; }
        public bool IsHQOrSupervisorOrInterviewer => InterviewerId.HasValue || SupervisorId.HasValue || HeadquartersId.HasValue;
        public Guid? InterviewerId { get; set; } 
        public Guid? SupervisorId { get; set; }
        public Guid? HeadquartersId { get; set; }
    }
}
