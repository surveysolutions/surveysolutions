using System;
using System.Linq;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks
{
    public abstract class BaseTask
    {
        protected readonly IScheduler scheduler;

        private readonly Type jobType;

        protected readonly JobKey jobKey;
        protected readonly TriggerKey triggerKey;

        protected BaseTask(IScheduler scheduler, string groupName, Type jobType)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            this.jobKey = new JobKey($"{groupName} job", groupName);
            this.jobType = jobType;

            this.triggerKey = new TriggerKey($"{groupName} trigger", groupName);
        }

        public virtual bool IsJobRunning() =>
            this.scheduler.GetCurrentlyExecutingJobs().FirstOrDefault(x => Equals(x.JobDetail.Key, jobKey)) != null;

        public virtual void Run(int intervalInSeconds = 1)
        {
            if (!this.scheduler.CheckExists(jobKey))
            {
                IJobDetail job = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();

                this.scheduler.AddJob(job, true);
            }

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(intervalInSeconds, IntervalUnit.Second))
                .Build();

            this.scheduler.ScheduleJob(trigger);
        }
    }
}
