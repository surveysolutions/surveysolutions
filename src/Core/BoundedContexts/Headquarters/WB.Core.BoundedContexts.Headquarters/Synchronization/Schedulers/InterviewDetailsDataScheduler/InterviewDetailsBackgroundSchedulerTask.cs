using System;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsBackgroundSchedulerTask : BaseTask
    {
        public InterviewDetailsBackgroundSchedulerTask(IScheduler scheduler) : base(scheduler, "interview packages ", typeof(SyncPackagesReprocessorBackgroundJob))
        {
        }
    }
}
