namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModelArg
    {
        public FinishInstallationViewModelArg(InterviewerIdentity userIdentity)
        {
            this.UserIdentity = userIdentity;
        }

        public InterviewerIdentity UserIdentity { get; set; }
    }
}
