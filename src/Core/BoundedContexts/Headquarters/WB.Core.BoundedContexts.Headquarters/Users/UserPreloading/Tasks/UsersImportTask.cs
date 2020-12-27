using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks
{
    public class UsersImportTask : BaseTask
    {
        public UsersImportTask(ISchedulerFactory schedulerFactory) : base(schedulerFactory, "Import users", typeof(UsersImportJob)) { }
    }
}
