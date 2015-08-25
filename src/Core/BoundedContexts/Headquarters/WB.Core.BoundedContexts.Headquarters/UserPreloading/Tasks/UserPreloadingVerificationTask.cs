using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UserPreloadingVerificationTask
    {
        readonly IScheduler scheduler;

        private readonly UserPreloadingSettings userPreloadingSettings;

        public UserPreloadingVerificationTask(IScheduler scheduler, UserPreloadingSettings userPreloadingSettings)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            this.scheduler = scheduler;
            this.userPreloadingSettings = userPreloadingSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UserPreloadingVerificationJob>()
                .WithIdentity("user preloading verification", "Batch user creation")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("user preloading verification trigger", "Batch user creation")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(userPreloadingSettings.VerificationIntervalInSeconds)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}