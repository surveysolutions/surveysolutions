using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UsersImportTask
    {
        private const string GroupName = "Import users";

        public static JobKey JobKey = new JobKey("import users job", GroupName);
        public static TriggerKey TriggerKey = new TriggerKey("import users trigger", GroupName);

        readonly IScheduler scheduler;

        private readonly UserPreloadingSettings userPreloadingSettings;

        public UsersImportTask(IScheduler scheduler, UserPreloadingSettings userPreloadingSettings)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.userPreloadingSettings = userPreloadingSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<UsersImportJob>()
                .WithIdentity(JobKey)
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(TriggerKey)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(userPreloadingSettings.ExecutionIntervalInSeconds)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}