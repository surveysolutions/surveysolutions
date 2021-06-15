using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Interviewer.Migrations.Workspaces
{
    [Migration(202106151642)]
    public class M202106151642_SetFlagToRecoveryInterviews: IMigration
    {
        private readonly IEnumeratorSettings settings;

        public M202106151642_SetFlagToRecoveryInterviews(
            IEnumeratorSettings settings)
        {
            this.settings = settings;
        }

        public void Up()
        {
            settings.SetDashboardViewsUpdated(false);
        }
    }
}