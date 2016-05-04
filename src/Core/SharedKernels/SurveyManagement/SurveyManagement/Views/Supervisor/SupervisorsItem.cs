using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Supervisor
{
    public class SupervisorsItem
    {
        public SupervisorsItem(Guid id, string name, string email, DateTime creationDate,
            bool isLockedBySupervisor, bool isLockedByHQ, int interviewersCount, 
            int notConnectedToDeviceInterviewersCount)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToShortDateString();
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
            this.InterviewersCount = interviewersCount;
            this.NotConnectedToDeviceInterviewersCount = notConnectedToDeviceInterviewersCount;
        }

        public bool IsLockedBySupervisor { get; private set; }

        public bool IsLockedByHQ { get; private set; }

        public string CreationDate { get; private set; }

        public string Email { get; private set; }

        public Guid UserId { get; private set; }

        public string UserName { get; private set; }

        public int InterviewersCount { get; private set; }

        public int NotConnectedToDeviceInterviewersCount { get; private set; }
    }
}