using System;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public class InterviewerSelectedArgs : EventArgs
    {
        public Guid InterviewerId { get; }
        public string Login { get; }
        public string FullName { get; }

        public InterviewerSelectedArgs(Guid interviewerId, string login, string fullName)
        {
            this.InterviewerId = interviewerId;
            this.Login = login;
            this.FullName = fullName;
        }
    }
}
