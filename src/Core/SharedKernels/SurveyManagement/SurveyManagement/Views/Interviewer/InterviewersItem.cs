using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersItem
    {
        #region Constructors and Destructors

        public InterviewersItem(Guid id, string name, string email, DateTime creationDate, bool isLockedBySupervisor, bool isLockedByHQ)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToShortDateString();
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
        }

        #endregion

        #region Public 

        public bool IsLockedBySupervisor { get; private set; }

        public bool IsLockedByHQ { get; private set; }

        public string CreationDate { get; private set; }

        public string Email { get; private set; }

        public Guid UserId { get; private set; }

        public string UserName { get; private set; }

        #endregion
    }
}