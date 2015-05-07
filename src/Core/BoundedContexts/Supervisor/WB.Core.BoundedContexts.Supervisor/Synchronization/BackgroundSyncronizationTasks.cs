using System;
using Quartz;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class BackgroundSyncronizationTasks
    {
        private readonly IScheduler scheduler;
        private readonly SchedulerSettings schedulerSettigns;

        public BackgroundSyncronizationTasks(IScheduler scheduler,
            SchedulerSettings schedulerSettigns)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            if (schedulerSettigns == null) throw new ArgumentNullException("schedulerSettigns");
            this.scheduler = scheduler;
            this.schedulerSettigns = schedulerSettigns;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<SynchronizationJob>()
                .WithIdentity("HQ sync", "Synchronization")
                .StoreDurably(true)
                .Build();

            if (this.schedulerSettigns.SchedulerEnabled)
            {
                // Trigger the job to run now, and then every 40 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("HQ sync trigger", "Synchronization")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(schedulerSettigns.HqSynchronizationInterval)
                        .RepeatForever())
                    .Build();

                this.scheduler.ScheduleJob(job, trigger);
            }

            this.scheduler.AddJob(job, true);
        }
    }
}