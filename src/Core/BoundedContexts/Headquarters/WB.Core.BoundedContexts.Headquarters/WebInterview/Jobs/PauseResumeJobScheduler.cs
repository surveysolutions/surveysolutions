using System;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs
{
    public class PauseResumeJobScheduler
    {
        private readonly IScheduler scheduler;

        public PauseResumeJobScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void Configure()
        {



            IJobDetail job1 = JobBuilder.Create<PrintUofs>()
                .WithIdentity("uof tracking", "WebInterview1")
                .StoreDurably(true)
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .WithIdentity("uof tracking identty", "WebInterview")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(5)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job1, trigger1);

            this.scheduler.AddJob(job1, true);





            IJobDetail job = JobBuilder.Create<PauseResumeJob>()
                .WithIdentity("pause resume job", "WebInterview")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("pause resume queue procesor", "WebInterview")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                .Build();

            this.scheduler.ScheduleJob(job, trigger);

            this.scheduler.AddJob(job, true);
        }
    }
}
