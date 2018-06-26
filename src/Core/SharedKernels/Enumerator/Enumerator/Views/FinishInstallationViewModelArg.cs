namespace WB.Core.SharedKernels.Enumerator.Views
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
