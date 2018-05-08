using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UsersImportTask : BaseTask
    {
        public UsersImportTask(IScheduler scheduler) : base(scheduler, "Import users", typeof(UsersImportJob)) { }
    }
}
