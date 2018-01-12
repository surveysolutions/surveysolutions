using System;
using System.Linq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public class UsersImportTask
    {
        const string GroupName = "Import users";

        readonly JobKey jobKey = new JobKey("import users job", GroupName);
        readonly TriggerKey triggerKey = new TriggerKey("import users trigger", GroupName);

        readonly IScheduler scheduler;

        public UsersImportTask(IScheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void Run()
        {
            if (!this.scheduler.CheckExists(jobKey))
            {
                IJobDetail job = JobBuilder.Create<UsersImportJob>()
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();

                this.scheduler.AddJob(job, true);
            }
            
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Second))
                .Build();

            this.scheduler.ScheduleJob(trigger);
        }

        public virtual bool IsJobRunning() =>
            this.scheduler.GetCurrentlyExecutingJobs().FirstOrDefault(x => Equals(x.JobDetail.Key, jobKey)) != null;
    }
}