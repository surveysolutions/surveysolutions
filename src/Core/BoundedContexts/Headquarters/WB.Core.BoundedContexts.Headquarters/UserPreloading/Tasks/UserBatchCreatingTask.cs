using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UserBatchCreatingTask
    {
        readonly IScheduler scheduler;

        private readonly UserPreloadingSettings userPreloadingSettings;

        public UserBatchCreatingTask(IScheduler scheduler, UserPreloadingSettings userPreloadingSettings)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
            this.userPreloadingSettings = userPreloadingSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UserBatchCreatingJob>()
                .WithIdentity("user creation", "Creation")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("user creation", "Creation")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(userPreloadingSettings.CreationIntervalInSeconds)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}