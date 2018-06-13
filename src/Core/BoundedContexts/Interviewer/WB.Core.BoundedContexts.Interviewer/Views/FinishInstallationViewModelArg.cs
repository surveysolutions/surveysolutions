namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModelArg
    {
        public FinishInstallationViewModelArg(string userName)
        {
            this.UserName = userName;
        }

        public string UserName { get; set; }
    }
}
