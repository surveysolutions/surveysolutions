using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks
{
    public class AssignmentsImportTask : BaseTask
    {
        public AssignmentsImportTask(ISchedulerFactory schedulerFactory) : base(schedulerFactory, "Import assignments", typeof(AssignmentsImportJob)) { }
    }
}
