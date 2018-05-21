using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class AssignmentsImportTask : BaseTask
    {
        public AssignmentsImportTask(IScheduler scheduler) : base(scheduler, "Import assignments", typeof(AssignmentsImportJob)) { }
    }
}
