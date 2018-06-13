using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class AssignmentsVerificationTask : BaseTask
    {
        public AssignmentsVerificationTask(IScheduler scheduler) : base(scheduler, "Verification assignments", typeof(AssignmentsVerificationJob)) { }
    }
}
