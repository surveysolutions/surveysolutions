using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersItem
    {
        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }

        public DateTime CreationDate { get; set; }

        public string Email { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string SupervisorName { get; set; }

        public string DeviceId { get; set; }
    }
}