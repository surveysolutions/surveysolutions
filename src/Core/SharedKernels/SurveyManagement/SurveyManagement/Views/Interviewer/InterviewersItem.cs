using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersItem
    {
        public InterviewersItem(Guid id, string name, string email, DateTime creationDate, bool isLockedBySupervisor, bool isLockedByHQ, string deviceId)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToShortDateString();
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
            this.DeviceId = deviceId;
        }

        public bool IsLockedBySupervisor { get; private set; }

        public bool IsLockedByHQ { get; private set; }

        public string CreationDate { get; private set; }

        public string Email { get; private set; }

        public Guid UserId { get; private set; }

        public string UserName { get; private set; }

        public string DeviceId { get; private set; }
    }
}