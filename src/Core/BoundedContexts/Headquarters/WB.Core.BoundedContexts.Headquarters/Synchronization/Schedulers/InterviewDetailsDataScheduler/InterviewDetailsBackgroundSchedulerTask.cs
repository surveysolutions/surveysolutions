using System;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsBackgroundSchedulerTask
    {
        private readonly IScheduler scheduler;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;

        public InterviewDetailsBackgroundSchedulerTask(IScheduler scheduler,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (interviewDetailsDataLoaderSettings == null) throw new ArgumentNullException(nameof(interviewDetailsDataLoaderSettings));
            this.scheduler = scheduler;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<InterviewDetailsBackgroundJob>()
                .WithIdentity("Capi interview packages sync", "Synchronization")
                .StoreDurably(true)
                .Build();


            if (this.interviewDetailsDataLoaderSettings.SchedulerEnabled)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("Capi interview packages sync trigger", "Synchronization")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(this.interviewDetailsDataLoaderSettings.SynchronizationInterval)
                        .RepeatForever())
                    .Build();

                this.scheduler.ScheduleJob(job, trigger);
            }

            this.scheduler.AddJob(job, true);
        }
    }
}