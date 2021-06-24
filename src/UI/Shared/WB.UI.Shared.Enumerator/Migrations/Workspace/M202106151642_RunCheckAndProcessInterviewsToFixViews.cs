using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Migrations.Workspace
{
    [Migration(202106151642)]
    public class M202106151642_RunCheckAndProcessInterviewsToFixViews: IMigration
    {
        private readonly IInterviewerInterviewAccessor accessor;

        public M202106151642_RunCheckAndProcessInterviewsToFixViews(
            IInterviewerInterviewAccessor accessor)
        {
            this.accessor = accessor;
        }

        public void Up()
        {
            accessor.CheckAndProcessInterviewsToFixViews();
        }
    }
}