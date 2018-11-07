using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersItem
    {
        public bool IsArchived { get; set; }
        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }

        public DateTime CreationDate { get; set; }

        public string Email { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string SupervisorName { get; set; }

        public string DeviceId { get; set; }
        public string EnumeratorVersion { get; set; }
        public int? EnumeratorBuild { get; set; }
        public Guid? SupervisorId { get; set; }
        public long TrafficUsed { get; set; }
    }
}
