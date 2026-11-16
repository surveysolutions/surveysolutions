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
            IJobDetail job = JobBuilder.Create()
                .OfType(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
            var scheduler = await this.schedulerFactory.GetScheduler();

            await scheduler.AddJob(job, new AddJobOptions { Replace = true });

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .StartAt(DateTimeOffset.Now.AddSeconds(rnd.Next(2, 15)))
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(repeatIntervalInSeconds)).RepeatForever())
                .Build();

            if (!await scheduler.Exists(triggerKey))
                await scheduler.ScheduleJob(trigger);
        }

        public virtual async Task ScheduleRunAsync(int startAtInSeconds = 1)
        {
            IJobDetail job = JobBuilder.Create()
                .OfType(jobType)
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
            
            var scheduler = await this.schedulerFactory.GetScheduler();

            await scheduler.AddJob(job, new AddJobOptions { Replace = true });

            var trigger = TriggerBuilder.Create()
                .WithDescription(jobKey.ToString())
                .ForJob(jobKey)
                .StartAt(DateTimeOffset.Now.AddSeconds(startAtInSeconds))
                .Build();

            await scheduler.ScheduleJob(trigger);
        }
    }
}
