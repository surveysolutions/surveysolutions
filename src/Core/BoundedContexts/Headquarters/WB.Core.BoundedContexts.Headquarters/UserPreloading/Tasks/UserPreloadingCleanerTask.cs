using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UserPreloadingCleanerTask
    {
        readonly IScheduler scheduler;

        private readonly UserPreloadingSettings userPreloadingSettings;

        public UserPreloadingCleanerTask(IScheduler scheduler, UserPreloadingSettings userPreloadingSettings)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
            this.userPreloadingSettings = userPreloadingSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UserPreloadingCleanerJob>()
                .WithIdentity("user preloading cleaner", "Batch user creation")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("user preloading cleaner trigger", "Batch user creation")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(userPreloadingSettings.CleaningIntervalInHours)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}