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

        public virtual void Schedule(int repeatIntervalInSeconds)
        {
            if (!this.scheduler.CheckExists(jobKey))
            {
                IJobDetail job = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();

                this.scheduler.AddJob(job, true);
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Second))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(repeatIntervalInSeconds).RepeatForever())
                .Build();

            if (!this.scheduler.CheckExists(triggerKey))
                this.scheduler.ScheduleJob(trigger);
        }

        public virtual void Run(int startAtInSeconds = 1)
        {
            if (!this.scheduler.CheckExists(jobKey))
            {
                IJobDetail job = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .Build();

                this.scheduler.AddJob(job, true);
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(new TriggerKey($"{triggerKey.Name}{this.scheduler.GetTriggersOfJob(jobKey).Count + 1}", triggerKey.Group))
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(startAtInSeconds, IntervalUnit.Second))
                .Build();

            this.scheduler.ScheduleJob(trigger);
        }
    }
}
