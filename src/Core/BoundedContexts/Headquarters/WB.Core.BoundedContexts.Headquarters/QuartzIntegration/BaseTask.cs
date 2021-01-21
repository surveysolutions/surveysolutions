using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
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
            this.scheduler.GetCurrentlyExecutingJobs().Result.FirstOrDefault(x => Equals(x.JobDetail.Key, jobKey)) != null;

        public virtual async Task Schedule(int repeatIntervalInSeconds)
        {
            IJobDetail job = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            await this.scheduler.AddJob(job, true);

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Second))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(repeatIntervalInSeconds).RepeatForever())
                .Build();

            if (!await this.scheduler.CheckExists(triggerKey))
                await this.scheduler.ScheduleJob(trigger);
        }

        public virtual async Task ScheduleRunAsync(int startAtInSeconds = 1)
        {
            IJobDetail job = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            await this.scheduler.AddJob(job, true);

            var trigger = TriggerBuilder.Create()
                .WithDescription(jobKey.ToString())
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(startAtInSeconds, IntervalUnit.Second))
                .Build();

            await this.scheduler.ScheduleJob(trigger);
        }
    }
}
