using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsBackgroundSchedulerTask : BaseTask
    {
        public InterviewDetailsBackgroundSchedulerTask(ISchedulerFactory schedulerFactory) : base(schedulerFactory, "interview packages ", typeof(SyncPackagesReprocessorBackgroundJob))
        {
        }
    }
}
