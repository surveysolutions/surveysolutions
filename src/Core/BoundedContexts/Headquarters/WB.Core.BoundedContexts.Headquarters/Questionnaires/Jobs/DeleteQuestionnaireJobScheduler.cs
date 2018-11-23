using System;
using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    public class DeleteQuestionnaireJobScheduler 
    {
        private readonly IScheduler scheduler;

        public DeleteQuestionnaireJobScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void Configure()
        {
            IJobDetail job = JobBuilder.Create<DeleteQuestionnaireJob>()
                .WithIdentity("Delete questionnaiers", "Import")
                .StoreDurably(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("Delete questionnaiers trigger", "Import")
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
