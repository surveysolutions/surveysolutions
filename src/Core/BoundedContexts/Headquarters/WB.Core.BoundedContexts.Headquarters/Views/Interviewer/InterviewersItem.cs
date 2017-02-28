using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersItem
    {
        public InterviewersItem(string id, string name, string supervisorName, string email, DateTime creationDate, bool isLockedBySupervisor, bool isLockedByHQ, string deviceId)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToLocalTime().FormatDateWithTime();
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
            this.DeviceId = deviceId;
            this.SupervisorName = supervisorName;
        }

        public bool IsLockedBySupervisor { get; private set; }

        public bool IsLockedByHQ { get; private set; }

        public string CreationDate { get; private set; }

        public string Email { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public string SupervisorName { get; private set; }

        public string DeviceId { get; private set; }
    }
}