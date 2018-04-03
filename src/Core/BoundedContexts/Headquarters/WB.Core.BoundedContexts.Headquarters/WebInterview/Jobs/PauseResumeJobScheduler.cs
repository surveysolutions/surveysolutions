using System;
using System.Threading.Tasks;
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

        public async Task ConfigureAsync()
        {
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

            await this.scheduler.ScheduleJob(job, trigger);

            await this.scheduler.AddJob(job, true);
        }
    }
}
