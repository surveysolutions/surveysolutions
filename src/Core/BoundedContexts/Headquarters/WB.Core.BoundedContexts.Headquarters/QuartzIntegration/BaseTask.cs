#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public abstract class BaseTask
    {
        public const string DATA_KEY = "_data_";
        protected readonly ISchedulerFactory schedulerFactory;


        private readonly Type jobType;

        protected readonly JobKey jobKey;
        protected readonly TriggerKey triggerKey;

        protected BaseTask(ISchedulerFactory schedulerFactory, string groupName, Type jobType)
        {
            this.schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));

            this.jobKey = new JobKey($"{groupName} job", groupName);
            this.jobType = jobType;

            this.triggerKey = new TriggerKey($"{groupName} trigger", groupName);
        }

        //public virtual bool IsJobRunning() =>
        //    this.scheduler.GetCurrentlyExecutingJobs().Result.FirstOrDefault(x => Equals(x.JobDetail.Key, jobKey)) != null;

        public virtual async Task Schedule(int repeatIntervalInSeconds)
        {
            IJobDetail job = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
            var scheduler = await this.schedulerFactory.GetScheduler();
            await scheduler.AddJob(job, true);

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Second))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(repeatIntervalInSeconds).RepeatForever())
                .Build();

            if (!await scheduler.CheckExists(triggerKey))
                await scheduler.ScheduleJob(trigger);
        }

        public virtual async Task ScheduleRunAsync(int startAtInSeconds = 1)
        {
            IJobDetail job = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
            
            var scheduler = await this.schedulerFactory.GetScheduler();

            await scheduler.AddJob(job, true);

            var trigger = TriggerBuilder.Create()
                .WithDescription(jobKey.ToString())
                .ForJob(jobKey)
                .StartAt(DateBuilder.FutureDate(startAtInSeconds, IntervalUnit.Second))
                .Build();

            await scheduler.ScheduleJob(trigger);
        }
    }
}
