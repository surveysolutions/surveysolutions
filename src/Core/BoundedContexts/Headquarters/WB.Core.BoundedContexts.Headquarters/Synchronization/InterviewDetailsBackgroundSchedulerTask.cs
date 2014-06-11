﻿using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization
{
    public class InterviewDetailsBackgroundSchedulerTask
    {
        private readonly IScheduler scheduler;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;

        public InterviewDetailsBackgroundSchedulerTask(IScheduler scheduler,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            if (interviewDetailsDataLoaderSettings == null) throw new ArgumentNullException("interviewDetailsDataLoaderSettings");
            this.scheduler = scheduler;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<InterviewDetailsDataLoader>()
                .WithIdentity("Capi interview packages sync", "Synchronization")
                .StoreDurably(true)
                .Build();


            if (this.interviewDetailsDataLoaderSettings.SchedulerEnabled)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("Capi interview packages sync trigger", "Synchronization")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(interviewDetailsDataLoaderSettings.SynchronizationInterval)
                        .RepeatForever())
                    .Build();

                this.scheduler.ScheduleJob(job, trigger);
            }

            this.scheduler.AddJob(job, true);
        }
    }
}