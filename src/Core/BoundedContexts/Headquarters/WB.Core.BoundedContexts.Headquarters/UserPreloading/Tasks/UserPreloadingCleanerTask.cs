using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UserPreloadingCleanerTask
    {
        readonly IScheduler scheduler;

        public UserPreloadingCleanerTask(IScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UserPreloadingCleanerJob>()
                .WithIdentity("user preloading cleaning", "Cleaning")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("user preloading cleaning", "Cleaning")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(12)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}