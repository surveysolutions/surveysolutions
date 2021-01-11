#nullable enable
using System;
using System.Threading.Tasks;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public abstract class BaseTask
    {
        public const string TaskDataKey = "_data_";
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

        private static Random rnd = new ();

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
                .StartAt(DateBuilder.FutureDate(rnd.Next(2,15), IntervalUnit.Second))
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
